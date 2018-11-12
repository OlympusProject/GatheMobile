using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;

namespace GatheApp
{
    public class WS
    {
        private static HttpClient CreateRestClient(string url, Boolean isUser)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                UseProxy = false
            };
            var client = new HttpClient(handler);

            if (Xamarin.Forms.Device.OS == Xamarin.Forms.TargetPlatform.iOS)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.5.17 (KHTML, like Gecko) Version/9.1 Safari/601.5.17");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "utf-8;q=0.7,*;q=0.7");
            }
            else
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; ru; rv:48.0) Gecko/20100101 Firefox/48.0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.7");
            }
            client.Timeout = TimeSpan.FromSeconds(8);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(Const.AcceptHeaderApplicationJson));
            if (isUser)
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Var.tokenModel.access_token);
            }
            return client;
        }

        //TODO: Step 2 - Call the web service and retrieve data
        private static async Task<T> GetData<T>(HttpClient client, string url, string parameter, Boolean isUser = true, bool cekUrl = true)
        {
            try
            {
                // cek url
                string newUrl = cekUrl ? GlobalFunc.cekUrl(url + parameter) : (url + parameter);

                Uri uri = new Uri(newUrl);
                var getDataResponse = await client.GetAsync(uri.AbsoluteUri, HttpCompletionOption.ResponseContentRead);
                //If we do not get a successful status code, then return an empty set
                if (!getDataResponse.IsSuccessStatusCode)
                {
                    if (getDataResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && isUser)
                    {
                        if (Var.tokenModel.access_token != null)
                        {
                            if (Var.tokenModel.refresh_token != null)
                            {
                                Var.tokenModel = await WS.PostDataAsync<TokenModel>(Const.BaseAddressWS, ConstUrl.auth_token + "?client_id=" + Const.client_id + "&client_secret=" + Const.client_secret + "&grant_type=refresh_token" + "&refresh_token=" + Var.tokenModel.refresh_token, null, true);
                                return await GetData<T>(CreateRestClient(url, isUser), url, parameter);
                            }
                        }
                    }
                    var error = await getDataResponse.Content.ReadAsStringAsync();
                    return default(T);
                }

                var jsonResponse = await getDataResponse.Content.ReadAsStringAsync();
                T jres = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonResponse);
                return jres;
            }
            catch (Exception ex)
            {
                CekTimeout(ex);
                return default(T);
            }
        }

        public static async Task<T> GetDataAsync<T>(string url, string parameter, bool cekUrl = true)
        {
            using (var client = CreateRestClient(url, false))
            {
                if (cekUrl)
                    return await GetData<T>(client, url, parameter, false, cekUrl);
                else
                    return await GetData<T>(client, url, parameter, false);
            }
        }

        //TODO: Step 2 - Call the web service and retrieve data
        public static async Task<T> GetDataAsyncUser<T>(string url, string parameter)
        {
            using (var client = CreateRestClient(url, true))
            {
                return await GetData<T>(client, url, parameter);
            }
        }

        //TODO: Step 2 - Call the web service and retrieve data
        private static async Task<String> PostData(HttpClient client, string url, string parameter, object obj, Boolean isUpdate, Boolean isUser = true, Boolean isRefreshToken = false, Boolean isPureObj = false)
        {
            try
            {
                HttpContent content = new StringContent("");
                Uri uri = new Uri(url + parameter);
                if (obj != null)
                {
                    content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj).ToString(), Encoding.UTF8, Const.AcceptHeaderApplicationJson);
                }
                HttpResponseMessage getDataResponse;
                if (isUpdate)
                {
                    getDataResponse = await client.PutAsync(uri.AbsoluteUri, content);
                }
                else
                {
                    getDataResponse = await client.PostAsync(uri.AbsoluteUri, content);
                }


                if (!getDataResponse.IsSuccessStatusCode)
                {
                    var error = await getDataResponse.Content.ReadAsStringAsync();
                    System.Console.Write(error);
                    if (getDataResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && isUser)
                    {
                        if (isRefreshToken)
                        {
                            Var.tokenModel = new TokenModel();
                            App.Current.MainPage.Navigation.PushAsync(new Sign_In(), false);
                        }
                        else
                        {
                            if (Var.tokenModel.access_token != null)
                            {
                                if (Var.tokenModel.refresh_token != null)
                                {
                                    Var.tokenModel = await WS.PostDataAsync<TokenModel>(Const.BaseAddressWS, ConstUrl.auth_token + "?client_id=" + Const.client_id + "&client_secret=" + Const.client_secret + "&grant_type=refresh_token" + "&refresh_token=" + Var.tokenModel.refresh_token, null, true);
                                    return await PostData(CreateRestClient(url, isUser), url, parameter, obj, isUpdate);
                                }
                            }
                        }
                    }
                    if (!isPureObj)
                        return Const.ResponseFailed;
                }
                var jsonresponse = await getDataResponse.Content.ReadAsStringAsync();
                return jsonresponse;
            }
            catch (Exception ex)
            {
                CekTimeout(ex);
                String msg = ex.Message;
                return Const.ResponseFailed;
            }
        }

        public static async Task<T> PostDataAsync<T>(string url, string parameter, object obj, Boolean isRefreshToken = false, Boolean isUpdate = false)
        {
            using (var client = CreateRestClient(url, false))
            {
                String response = await PostData(client, url, parameter, obj, isUpdate, false, isRefreshToken);
                if (response.Equals(Const.ResponseFailed))
                {
                    return default(T);
                }
                else
                {
                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response);
                    }
                    catch (Exception ex)
                    {
                        return default(T);
                    }

                }
            }
        }

        public static async Task<String> PostDataStringAsync(string url, string parameter, object obj, Boolean isPureObj = false)
        {
            using (var client = CreateRestClient(url, false))
            {
                return await PostData(client, url, parameter, obj, false, false, false, isPureObj);
            }
        }

        public static async Task<T> PostDataAsyncUser<T>(string url, string parameter, object obj, Boolean isUpdate)
        {
            using (var client = CreateRestClient(url, true))
            {
                String response = await PostData(client, url, parameter, obj, isUpdate);
                if (response.Equals(Const.ResponseFailed))
                {
                    return default(T);
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response);
                }
            }
        }

        public static async Task<String> PostDataStringAsyncUser(string url, string parameter, object obj, Boolean isUpdate = false)
        {
            using (var client = CreateRestClient(url, true))
            {
                return await PostData(client, url, parameter, obj, isUpdate);
            }
        }

        public static async Task<T> PostDataAsyncUserMultipartForm<T>(string url, string parameter, Dictionary<String, Object> postParameters, Boolean isUpdate, bool isUser = true)
        {
            using (var client = CreateRestClient(url, isUser))
            {
                client.Timeout = TimeSpan.FromSeconds(100);
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                string contentType = "multipart/form-data; boundary=" + formDataBoundary;
                try
                {
                    MultipartFormDataContent content = new MultipartFormDataContent(formDataBoundary);
                    foreach (var param in postParameters)
                    {
                        if (param.Value is FileParameter)
                        {
                            content.Add(new ByteArrayContent(((FileParameter)param.Value).File), param.Key, ((FileParameter)param.Value).FileName);
                        }
                        else
                        {
                            content.Add(new StringContent(string.Format("{0}", param.Value)), param.Key);
                        }
                    }

                    HttpResponseMessage getDataResponse;
                    Uri uri = new Uri(url + parameter);
                    if (isUpdate)
                    {
                        getDataResponse = await client.PutAsync(uri.AbsoluteUri, content);
                    }
                    else
                    {
                        getDataResponse = await client.PostAsync(uri.AbsoluteUri, content);
                    }

                    if (!getDataResponse.IsSuccessStatusCode)
                    {
                        String tes = await getDataResponse.Content.ReadAsStringAsync();
                        return default(T);
                    }

                    var jsonresponse = await getDataResponse.Content.ReadAsStringAsync();
                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonresponse);
                    }
                    catch (Exception ex)
                    {
                        return default(T);
                    }
                }
                catch (Exception ex)
                {
                    CekTimeout(ex);
                    String msg = ex.Message;
                    return default(T);
                }
            }
        }

        //TODO: Step 2 - Call the web service and retrieve data
        private static async Task<String> DeleteData(HttpClient client, string url, string parameter, Boolean isRefreshToken = false)
        {
            try
            {
                string newUrl = GlobalFunc.cekUrl(url + parameter);
                Uri uri = new Uri(newUrl);
                HttpResponseMessage getDataResponse = await client.DeleteAsync(uri.AbsoluteUri);
                if (!getDataResponse.IsSuccessStatusCode)
                {
                    if (getDataResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        if (isRefreshToken)
                        {
                            Var.tokenModel = new TokenModel();
                            App.Current.MainPage.Navigation.PushAsync(new Sign_In(), false);
                        }
                        else
                        {
                            if (Var.tokenModel.access_token != null)
                            {
                                if (Var.tokenModel.refresh_token != null)
                                {
                                    Var.tokenModel = await WS.PostDataAsync<TokenModel>(Const.BaseAddressWS, "api/auth/token?client_id=" + Const.client_id + "&client_secret=" + Const.client_secret + "&grant_type=refresh_token" + "&refresh_token=" + Var.tokenModel.refresh_token, null, true);
                                    return await DeleteData(CreateRestClient(url, true), url, parameter);
                                }
                                else { App.Current.MainPage.Navigation.PushAsync(new Sign_In(), false); }
                            }
                            else { App.Current.MainPage.Navigation.PushAsync(new Sign_In(), false); }
                        }
                    }
                    return Const.ResponseFailed;
                }
                var jsonresponse = await getDataResponse.Content.ReadAsStringAsync();
                return jsonresponse;
            }
            catch (Exception ex)
            {
                CekTimeout(ex);
                String msg = ex.Message;
                return Const.ResponseFailed;
            }
        }

        public static async Task<T> DeleteDataAsyncUser<T>(string url, string parameter, Boolean isRefreshToken = false)
        {
            using (var client = CreateRestClient(url, true))
            {
                String response = await DeleteData(client, url, parameter);
                if (response.Equals(Const.ResponseFailed))
                {
                    return default(T);
                }
                else
                {
                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response);
                    }
                    catch (Exception ex)
                    {
                        return default(T);
                    }
                }
            }
        }

        public static async Task<String> DeleteDataStringAsyncUser<T>(string url, string parameter, Boolean isRefreshToken = false)
        {
            using (var client = CreateRestClient(url, true))
            {
                String response = await DeleteData(client, url, parameter);
                if (response.Equals(Const.ResponseFailed))
                {
                    return Const.ResponseFailed;
                }
                else
                {
                    return response;
                }
            }
        }

        public static bool CekTimeout(Exception ex)
        {
            Type type = ex.GetType();
            if (type.Equals(typeof(TaskCanceledException)))
            {
                var taskEx = (TaskCanceledException)ex;
                if (taskEx.Task.Status.ToString() == "Canceled")
                {
                    GlobalFunc.showPopupError("Request Timeout", "Please check your internet connection.", "Try Again");
                    return true;
                }
            }
            return false;
        }
    }
}
