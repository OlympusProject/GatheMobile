using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;
using Plugin.Toasts;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Linq;
using GatheApp.Class;
using Xamarin;
using static Xamarin.Insights;
using Rg.Plugins.Popup.Extensions;
using System.Reflection;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Platform.Services;

namespace GatheApp
{
    public class GlobalFunc
    {
        public static string ToAMPM(TimeSpan date, bool withSpace = false)
        {
            string output = "";
            string space = (withSpace) ? " " : "";
            DateTime time = DateTime.Today.Add(date);
            if (time.Minute == 0)
            {
                output = time.ToString("h" + space + "tt");
            }
            else
            {
                output = time.ToString("h:mm" + space + "tt");
            }
            return output;
        }

        public static string SuffixDate(DateTime date)
        {
            string getDate = date.ToString("dd");
            int number = Int32.Parse(getDate);
            string suffix = String.Empty;

            int ones = number % 10;
            int tens = (int)Math.Floor(number / 10M) % 10;

            if (tens == 1)
            {
                suffix = "th";
            }
            else
            {
                switch (ones)
                {
                    case 1:
                        suffix = "st";
                        break;

                    case 2:
                        suffix = "nd";
                        break;

                    case 3:
                        suffix = "rd";
                        break;

                    default:
                        suffix = "th";
                        break;
                }
            }
            string output = "";
            output = date.ToString("MMMM") + " " + String.Format("{0}{1}", number, suffix) + ", " + date.ToString("yyyy");
            return output;
        }

        // xamarin insights report
        public static void InsightsReport(Exception ex, string method = "", string userId = "")
        {
            ExecInsightsReport(ex, method, userId, Severity.Error);
        }

        public static void InsightsReport(Exception ex)
        {
            ExecInsightsReport(ex, "", "", Severity.Error);
        }

        public static void InsightsReport(Exception ex, Severity warningLevel)
        {
            ExecInsightsReport(ex, "", "", warningLevel);
        }

        public static void InsightsReport(Exception ex, string method, string userId, Severity warningLevel)
        {
            ExecInsightsReport(ex, method, userId, Severity.Error);
        }

        private static void ExecInsightsReport(Exception exp, string method, string userId, Severity warningLevel)
        {
            string str = "";
            try
            {
                str = System.Environment.MachineName;
            }
            catch (Exception)
            {
                str = "-";
            }
            method = method == null ? "" : method;
            userId = userId == null ? "" : userId;
            if (warningLevel != Severity.Warning)
            {
                Insights.Report(exp, new Dictionary<String, String> {
                    { "Class", exp.TargetSite.DeclaringType.Name },
                    { "Void", method=="-"?exp.TargetSite.Name:method },
                    { "UserId", userId==""?"-":userId },
                    { "MachineName", str }
                }, warningLevel);
            }
        }

        public static void Debug(string message)
        {
            System.Diagnostics.Debug.Write(Device.OnPlatform("iOS", "Android", "WinPhone") + " - " + message);
        }

        public static CekValidURLResponse CekValidURL(string url)
        {
            string msg = "";
            bool cek = false;
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 15000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                request.Method = "HEAD"; //Get only the header information -- no need to download any content
                                         //request.AllowAutoRedirect = false; //set no redirect if not found
                response = request.GetResponse() as HttpWebResponse;

                cek = response.StatusCode == HttpStatusCode.OK;
                msg = response.StatusDescription;
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    msg = ex.Message;
                }
                else
                {
                    var res = (ex.Response as HttpWebResponse);
                    msg = ((int)res.StatusCode) + " - " + res.StatusDescription;
                }
                cek = false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                cek = false;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return (new CekValidURLResponse { Status = cek, Description = msg });
        }

        // Convert an object to a byte array
        public static byte[] StreamToByteArray(Stream sourceStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                sourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public static byte[] ObjectToByteArray(Object obj)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }

            }
            catch (Exception ex) { Debug(ex.Message + "\n" + ex.StackTrace); return null; }
        }

        // ccek url
        public static string cekUrl(string url)
        {
            string newUrl = url;
            try
            {
                if (newUrl.Contains(Const.BaseAddressWS))
                {
                    string txt = "";
                    // cek tanda '/' sebelum '?'
                    if (newUrl.Contains("?"))
                    {
                        int index = newUrl.IndexOf('?');
                        string str = newUrl.Substring(index - 1, 1);
                        if (str != "/")
                        {
                            string fisrtString = newUrl.Substring(0, index);
                            string secondString = newUrl.Substring(index, newUrl.Length - index);
                            newUrl = fisrtString + "/" + secondString;
                        }
                        return newUrl;
                    }

                    // cek tanda '/' di akhir url+parameter
                    txt = newUrl.Substring(newUrl.Length - 1, 1);
                    if (txt != "/")
                    {
                        newUrl = newUrl + "/";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            return newUrl;
        }

        // check connection only
        private static bool checkConnection()
        {
            var currentDevice = Resolver.Resolve<IDevice>();
            try
            {
                var networkStatus = currentDevice.Network.InternetConnectionStatus();
                if (networkStatus == NetworkStatus.NotReachable)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                var exception = ex.Message + ": " + ex.StackTrace;
                return true;
            }
        }

        // check internet connection and redirect if return false
        // parameter redirect always set to true
        public static bool CheckForInternetConnection(bool redirect = true)
        {
            if (!checkConnection())
            {
                if (redirect)
                {
                    // redirect to welcome page
                    App.Current.MainPage.Navigation.PushAsync(new Welcome());
                    return false;
                }
                else
                    return false;
            }
            else
                return true;
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        public static async Task<UserModel> refreshCurrUser()
        {
            String param = "user/current/";
            List<UserModel> userModelList = await WS.GetDataAsyncUser<List<UserModel>>(Const.BaseAddressWS, param);
            if (userModelList != null)
            {
                if (userModelList.Count > 0)
                {
                    Var.user_model = userModelList[0];
                    return userModelList[0];
                }
            }
            return null;
        }

        public static async Task refreshCityCountryState()
        {
            List<CityModel> cityList = await WS.GetDataAsync<List<CityModel>>(Const.BaseAddressWS, ConstUrl.city);
            if (cityList != null)
            {
                if (cityList.Count > 0)
                {
                    Var.cityList = cityList;
                }
            }
            List<CountryModel> countryList = await WS.GetDataAsync<List<CountryModel>>(Const.BaseAddressWS, ConstUrl.country);
            if (countryList != null)
            {
                if (countryList.Count > 0)
                {
                    Var.countryList = countryList;
                }
            }
            List<StateModel> stateList = await WS.GetDataAsync<List<StateModel>>(Const.BaseAddressWS, ConstUrl.state);
            if (stateList != null)
            {
                if (stateList.Count > 0)
                {
                    Var.stateList = stateList;
                }
            }
        }

        public static async void registerDevice()
        {
            try
            {
                int user_id = Var.user_model.id;
                String param = "user/device/";
                UserDeviceModel userDevice = new UserDeviceModel(user_id, App.device_id);
                List<UserDeviceModel> userDeviceList = await WS.GetDataAsyncUser<List<UserDeviceModel>>(Const.BaseAddressWS, param + "?device_id=" + App.device_id);
                if (userDeviceList.Count > 0)
                {
                    if (userDeviceList.Count > 1)
                    {
                        foreach (UserDeviceModel userDeviceModel in userDeviceList)
                        {
                            String response = await WS.DeleteDataAsyncUser<String>(Const.BaseAddressWS, param + userDeviceModel.id + "/");
                        }
                        insertDevice(param, userDevice);
                    }
                    else
                    {
                        if (userDeviceList[0].user != userDevice.user)
                        {
                            String response = await WS.DeleteDataAsyncUser<String>(Const.BaseAddressWS, param + userDeviceList[0].id + "/");
                            insertDevice(param, userDevice);
                        }
                    }
                }
                else
                {
                    insertDevice(param, userDevice);
                }
            }
            catch (Exception ex)
            {
                InsightsReport(ex, "register device");
            }
        }

        private static async void insertDevice(String url, UserDeviceModel userDevice)
        {
            try
            {
                UserDeviceModel userModelList = await WS.PostDataAsyncUser<UserDeviceModel>(Const.BaseAddressWS, url, userDevice, false);
            }
            catch { }
        }

        public static bool CekServiceEstablished()
        {
            var cek = CekValidURL(Const.BaseAddressWS + "status/");
            return cek.Status;
        }

        public static Xamarin.Forms.ImageSource resizedImageSource(Xamarin.Forms.ImageSource source, double width, double height, bool resizeImg = false)
        {
            try
            {
                if (source.GetType() == typeof(FileImageSource))
                {
                    var _source = ((FileImageSource)source).File;
                    if (resizeImg)
                    {
                        var resize = DependencyService.Get<IResizeImage>();
                        var assembly = typeof(App).GetTypeInfo().Assembly; // you can replace "this.GetType()" with "typeof(MyType)", where MyType is any type in your assembly.
                        byte[] buffer;
                        try
                        {
                            using (Stream s = assembly.GetManifestResourceStream(_source))
                            {
                                var strm = resize.ResizeImage(GlobalFunc.StreamToByteArray(s), Math.Ceiling(height * App.Density * 2), Math.Ceiling(width * App.Density * 2), _source);
                                buffer = strm;
                            }
                            source = ImageSource.FromStream(() => new MemoryStream(buffer));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Print(Device.OnPlatform("iOS", "Android", "WinPhone") + " - " + ex.Message);
                        }
                    }
                    else
                    {
                        source = ImageSource.FromFile(_source);
                    }
                }
                else if (source.GetType() == typeof(UriImageSource))
                {
                    string _source = WebUtility.UrlEncode(((UriImageSource)source).Uri.AbsoluteUri);
                    Uri uri = new Uri(Const.BaseAddressImageResize + "?url=" + _source + "&no_expand=1&container=focus&resize_w=" + Math.Ceiling(width * App.Density * 2) + "&resize_h=" + Math.Ceiling(height * App.Density * 2) + "&refresh=3600000");
                    source = uri.AbsoluteUri;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            return source;
        }


        public static bool IsValidNumber(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn, "^[0-9]*$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool isValidNumberWithRange(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn, "^([0-9]+|([0-9]+)-([0-9]+))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static bool IsValidTime(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn, "^(?:[01][0-9]|2[0-3]):[0-5][0-9]$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string getDaysDiff(DateTime date)
        {
            String result = "";
            DateTime now = DateTime.Now.ToLocalTime();
            int seconds_dif = (int)(now - date).TotalSeconds;
            result = seconds_dif + "s";
            if (seconds_dif > 60)
            {
                int minutes_dif = (int)(now - date).TotalMinutes;
                result = minutes_dif + "m";
                if (minutes_dif > 60)
                {
                    int hours_dif = (int)(now - date).TotalHours;
                    result = hours_dif + "h";
                    if (hours_dif > 24)
                    {
                        int days_dif = (int)(now - date).TotalDays;
                        result = days_dif + "d";
                        if (days_dif > 7)
                        {
                            int weeks_dif = days_dif / 7;
                            result = weeks_dif + "w";
                        }
                    }
                }
            }
            else if (seconds_dif < 0)
            {
                result = "";
            }

            return result;
        }

        static bool invalid = false;
        public static bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static String IsValidUrl(string url)
        {
            string[] prefixes = { "http://", "https://" };
            bool result = prefixes.Any(prefix => url.StartsWith(prefix));
            if (!result)
            {
                url = "http://" + url;
            }
            string pattern = @"((([A - Za - z]{ 3,9}:(?:\/\/)?)(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+)((?:\/[\+~%\/\.\w\-_]*)?\??(?:[\-\+=&;%@\.\w_]*)#?(?:[\.\!\/\\\w]*))?)$";
            if (Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
            {
                return url;
            }
            else
            {
                return AppResources.InvalidUrl;
            }
        }

        public static bool CheckSpecialChar(string text)
        {
            //String pattern = @"^[^-_#]+$";
            //String pattern = @"^[a-zA-Z0-9\-#_]$";
            //String pattern = @"[^a-zA-Z0-9-#_\-\&\,\' ]";
            String pattern = @"[^a-zA-Z0-9-#_\-\+\&\,\'\!\?\(\)\/\$\%\*\@ ]";//alfanumerik dan #_-&,'!?()/$%*@
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        public static string ReplaceURl(Match m)
        {
            return "<a style='text-decoration:none' href='search:" + m.ToString().Substring(1) + "'>" + m.ToString() + "</a>";
        }

        public static double getDistancePythagoras(double lat1, double long1, double lat2, double long2)
        {
            double diffLat = lat1 - lat2;
            double diffLong = long1 - long2;
            double dist = Math.Sqrt(diffLat * diffLat + diffLong * diffLong); //Math.Sqrt panggil rumus pythagoras

            return dist;
        }


        public static double getDistance(double lat1, double long1, double lat2, double long2)
        {
            double theta = long1 - long2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515; // distance coordinat to miles
            dist = dist * 1.609344; //miles to kilometer

            return (dist);
        }

        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        public static Color getColorWithAlpha(Color color, double alpha)
        {
            return new Color(color.R, color.G, color.B, alpha);
        }

        public static void setCacheObject<T>(String key, T obj)
        {
            DependencyService.Get<IServices>().setCacheObject<T>(key, obj);
        }

        public static T getCacheObject<T>(String key)
        {
            return DependencyService.Get<IServices>().getCacheObject<T>(key);
        }

        public static double ConvertHeight(double height)
        {
            double heightConverted = (App.ScreenHeight / 1334) * height;
            return heightConverted;
        }

        public static double ConvertWidth(double width)
        {
            double widthConverted = (App.ScreenWidth / 750) * width;
            return widthConverted;
        }

        public static double ConvertFontSize(double fontSize)
        {
            double fontSizeConverted = (App.ScreenHeight / 1334) * fontSize;
            return fontSizeConverted;
        }

    }

    public class ReturnStatus
    {
        public bool Status { get; private set; } = true;
        public string Message { get; private set; } = "";

        public void Set(bool status, string message)
        {
            this.Status = status;
            this.Message = string.IsNullOrWhiteSpace(message) ? "" : message;
        }
    }

    public class CekValidURLResponse
    {
        public bool Status { get; set; } = false;
        public string Description { get; set; } = "404";
    }
}
