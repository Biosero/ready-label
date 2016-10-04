using APISamples.Driver.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace APISamples.Driver
{
    public class ReadyLabelDriver
    {
        private string ipAddress;
        private int port;

        private string RootURL { get { return "http://" + ipAddress + ":" + port.ToString() + "/readylabel/"; } }

        /// <summary>
        /// Initializes the driver at the specified endpoint.
        /// </summary>
        /// <param name="address">The URL address.</param>
        /// <param name="portNumber">The listener port number.</param>
        /// <exception cref="Exception"></exception>
        public void InitializeDriver(string address, int portNumber)
        {
            try
            {
                ipAddress = address;
                port = portNumber;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Gets all the label templates
        /// </summary>
        /// <returns>
        /// An enumerable of <see cref="LabelTemplate"/> items
        /// </returns>
        public IEnumerable<LabelTemplate> GetTemplates()
        {
            IEnumerable<LabelTemplate> templates = SendGETRequest<IEnumerable<LabelTemplate>>(RootURL + "labels");
            return templates;
        }

        /// <summary>
        /// Gets the sequence data with the associated name.
        /// </summary>
        /// <param name="name">The name of the sequence.</param>
        /// <returns>
        /// The current value of the sequence.
        /// </returns>
        public string GetSequenceData(string name)
        {
            SequenceInfo info = SendGETRequest<SequenceInfo>(RootURL + "sequences/" + name.Replace(" ", "%20"));
            return info.CurrentDisplayData;
        }

        /// <summary>
        /// Increments the sequence with the associated name.
        /// </summary>
        /// <param name="name">The name of the sequence.</param>
        /// <returns>
        /// The current value of the sequence after being incremented.
        /// </returns>
        public string IncrementSequence(string name)
        {
            SequenceInfo info = SendPOSTRequest<SequenceInfo>(RootURL + "sequences/" + name.Replace(" ", "%20") + "/manager?action=increment", null);
            return info.CurrentDisplayData;
        }

        /// <summary>
        /// Initializes the EGecko printer
        /// </summary>
        public void EGeckoInitializeInstrument()
        {
            SendPOSTRequest<object>(RootURL + "printers/egecko/command?name=initialize", null);

            Thread.Sleep(1000);

            WaitWhileBusy();
        }

        /// <summary>
        /// Rotates the EGecko stage to the specifed position.
        /// </summary>
        /// <param name="rotation">The rotation value.</param>
        /// <param name="mode">The rotate mode (Absolute or Relative).</param>
        public void EGeckoRotateStage(double rotation, string mode)
        {
            SendPOSTRequest<RotateStageCommand>(RootURL + "printers/egecko/command?name=rotate", new RotateStageCommand()
            {
                Rotation = rotation,
                MovementMode = mode
            });

            Thread.Sleep(1000);

            WaitWhileBusy();
        }

        /// <summary>
        /// Send the command for the EGecko to print and apply to a plate on the specifed sides.
        /// </summary>
        /// <param name="fileName">Name of the label template file.</param>
        /// <param name="labelSides">The sides of the plate to label.</param>
        /// <param name="data">The data to pass to the label template.</param>
        /// <param name="pickupHeight">Height of the pickup.</param>
        /// <param name="applyHeight">Height of the apply.</param>
        /// <param name="applyDepth">The apply depth.</param>
        /// <param name="retryMissed">if set to <c>true</c> [retry missed].</param>
        /// <param name="retryCount">The retry count.</param>
        /// <returns>
        /// The result of the print and apply
        /// </returns>
        public bool EGeckoPrint(string fileName, Dictionary<string, bool> labelSides, Dictionary<string, string> data, double pickupHeight, double applyHeight, double applyDepth, bool retryMissed, int retryCount = 1)
        {
            //send print configuration settings
            SendPOSTRequest<EGeckoConfiguration>(RootURL + "printers/egecko", new EGeckoConfiguration()
            {
                IsValidating = true,
                PickupHeight = pickupHeight,
                ApplyHeight = applyHeight,
                ApplyDepth = applyDepth,
                ApplyNorth = labelSides["North"],
                ApplySouth = labelSides["South"],
                ApplyEast = labelSides["East"],
                ApplyWest = labelSides["West"]
            });

            Thread.Sleep(1000);

            //post print command
            SendPOSTRequest<PrintCommand>(RootURL + "printers/egecko/command?name=printandapply", new PrintCommand()
            {
                LabelFile = fileName,
                Data = data
            });

            Thread.Sleep(1000);

            WaitWhileBusy();

            //if retrying check for missed scan
            if (retryMissed)
            {
                EGeckoPrinterStatus status = SendGETRequest<EGeckoPrinterStatus>(RootURL + "printers/egecko/status?filter=state,errors");

                // this logic is untested
                //   -> recursive logic could have performance impacts
                foreach (var item in labelSides)
                {
                    if (!status.LastValidationEvent.BarcodesScanned.Keys.Contains(item.Key) || status.LastValidationEvent.BarcodesScanned[item.Key] != "expected result")
                    {
                        if (retryCount > 0)
                        {
                            return EGeckoPrint(fileName, labelSides, data, pickupHeight, applyHeight, applyDepth, retryMissed, retryCount--); //recursively retry print
                        }
                        return false;
                    }
                }
            }
            return true;       
        }

        /// <summary>
        /// Waits the while the printer status is busy.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void WaitWhileBusy()
        {
            PrinterStatus status = new PrinterStatus() { State = PrinterState.Busy };
            do
            {
                status = SendGETRequest<PrinterStatus>(RootURL + "printers/egecko/status?filter=state,errors");
                Thread.Sleep(500);
            } while (status.State == PrinterState.Busy);

            if (status.State == PrinterState.Errored)
            {
                throw new InvalidOperationException("Your request encountered the following errors: " + Environment.NewLine + string.Join(Environment.NewLine, status.Errors));
            }
        }

        /// <summary>
        /// Sends the GET request to the specified URL and returns the deserialized result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL to GET from</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private T SendGETRequest<T>(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                if ((int)response.StatusCode >= 300)
                {
                    throw new InvalidOperationException("Error " + response.StatusCode.ToString() + ": " + response.StatusDescription);
                }

                string body = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return JsonConvert.DeserializeObject<T>(body);
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    var response = (HttpWebResponse)(ex as WebException).Response;

                    throw new InvalidOperationException("Error " + response.StatusCode.ToString() + ": " + response.StatusDescription);
                }
                throw new InvalidOperationException(ex.Message);
            }
        }

        /// <summary>
        /// Sends the POST request to the specified URL and returns the deserialized result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL to POST to</param>
        /// <param name="postObject">The object to send in the POST body.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private T SendPOSTRequest<T>(string url, T postObject)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                //request.ContentType = "application/json; charset=utf-8";

                if (postObject != null)
                {
                    string postBody = JsonSerialize(postObject);
                    var data = Encoding.ASCII.GetBytes(postBody);

                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }


                var response = (HttpWebResponse)request.GetResponse();

                if ((int)response.StatusCode >= 300)
                {
                    throw new InvalidOperationException("Error " + response.StatusCode.ToString() + ": " + response.StatusDescription);
                }

                string body = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return JsonConvert.DeserializeObject<T>(body);
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    var webexception = (ex as WebException);
                    var response = (HttpWebResponse)webexception.Response;

                    if (response != null)
                    {
                        throw new InvalidOperationException("Error " + response.StatusCode.ToString() + ": " + response.StatusDescription);
                    }
                }
                throw new InvalidOperationException(ex.Message);
            }
        }

        /// <summary>
        /// Serializes the Json object for HTTP request
        /// </summary>
        /// <param name="o">The object to serialize</param>
        /// <returns>The Json result</returns>
        private string JsonSerialize(object o)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.SerializeObject(o, Formatting.Indented, settings);
        }
    }
}
