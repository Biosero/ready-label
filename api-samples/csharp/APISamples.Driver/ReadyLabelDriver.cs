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

        public IEnumerable<LabelTemplate> GetTemplates()
        {
            IEnumerable<LabelTemplate> templates = SendGETRequest<IEnumerable<LabelTemplate>>(RootURL + "labels");
            return templates;
        }

        public string GetSequenceData(string name)
        {
            SequenceInfo info = SendGETRequest<SequenceInfo>(RootURL + "sequences/" + name.Replace(" ", "%20"));
            return info.CurrentDisplayData;
        }

        public string IncrementSequence(string name)
        {
            SequenceInfo info = SendPOSTRequest<SequenceInfo>(RootURL + "sequences/" + name.Replace(" ", "%20") + "/manager?action=increment", null);
            return info.CurrentDisplayData;
        }

        public void EGeckoInitializeInstrument()
        {
            SendPOSTRequest<object>(RootURL + "printers/egecko/command?name=initialize", null);

            Thread.Sleep(1000);

            WaitWhileBusy();
        }

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

        public void EGeckoPrint(string fileName, Dictionary<string, bool> labelSides, Dictionary<string, string> data, double pickupHeight, double applyHeight, double applyDepth, bool retryMissed)
        {
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

            SendPOSTRequest<PrintCommand>(RootURL + "printers/egecko/command?name=printandapply", new PrintCommand()
            {
                LabelFile = fileName,
                Data = data
            });

            Thread.Sleep(1000);

            WaitWhileBusy();

            if (retryMissed)
            {
                EGeckoPrinterStatus status = SendGETRequest<EGeckoPrinterStatus>(RootURL + "printers/egecko/status?filter=state,errors");

                // this logic is untested so be wary of that 
                // also since its recursive it will never stop retrying to print
                foreach (var item in labelSides)
                {
                    if (!status.LastValidationEvent.BarcodesScanned.Keys.Contains(item.Key))
                    {
                        EGeckoPrint(fileName, labelSides, data, pickupHeight, applyHeight, applyDepth, retryMissed);
                    }
                    else
                    {
                        if (status.LastValidationEvent.BarcodesScanned[item.Key] != "expected result") //replace expected result with whatever you expected to see here
                        {
                            EGeckoPrint(fileName, labelSides, data, pickupHeight, applyHeight, applyDepth, retryMissed);
                        }
                    }
                }
            }       
        }

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
