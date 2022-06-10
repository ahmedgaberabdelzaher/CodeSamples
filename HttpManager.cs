using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Polly;
using Xamarin.Essentials;
using DeliveryManagement.Resources;
using Prism.Navigation;
using DryIoc;
using Prism.Ioc;
using DeliveryManagement.Views;
using System.Diagnostics;
using Prism.Services;
using Microsoft.AppCenter.Crashes;

namespace DeliveryManagement.Helpers
{
    public static class HttpManager
    {


        public static INavigationService AppNavigationService => (Application.Current as App).Container.Resolve<INavigationService>();
        public static IPageDialogService _dialogService => (Application.Current as App).Container.Resolve<IPageDialogService>();
        static HttpClient client = new HttpClient() { Timeout = new TimeSpan(0, 0, 20) };

        static App app = Application.Current as App;
        static Stopwatch _stopwatch = new Stopwatch();
        public static async Task<Tuple<T, bool, string>> GetListAsync<T>(string requestUrl, bool isShowAlert = false) where T : class
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    var client = new System.Net.Http.HttpClient();
                    System.Diagnostics.Debug.WriteLine($"======================GET:  {requestUrl} =====================");
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    client.DefaultRequestHeaders.Add("Accept-Language", lang);


                    System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    //System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = client.GetAsync(requestUrl).GetAwaiter().GetResult();
                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            var JsonObject = JsonConvert.DeserializeObject<T>(responseJson);
                            return Tuple.Create(JsonObject, true, "");
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            LogoutWhenUnAuthorized(response);
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.unAuthoorizaedMsg);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.unAuthoorizaedMsg);
                        }
                    }
                    else
                    {
                        return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.ServerErrorOrNoInternetConnection);
                    }

                }
                else
                {
                    return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.NoInternet);
                }

            }
            catch (System.Exception exp)
            {
                Crashes.TrackError(exp);
                return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.ServerErrorOrNoInternetConnection);
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }
        }

        public static async Task<Tuple<T, bool, string>> GetAsync<T>(string requestUrl, bool isShowAlert = false) where T : class
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    System.Diagnostics.Debug.WriteLine($"======================GET: {requestUrl} =====================");
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    client.DefaultRequestHeaders.Clear();
                    var token = "bearer" + " " + Preferences.Get("UserToken", "");
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    client.DefaultRequestHeaders.Add("Accept-Language", lang);


                    System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    System.Diagnostics.Debug.WriteLine($"Authorization: {token}");

                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 3,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");

                        var resultJson = await client.GetAsync(requestUrl);

                        return resultJson;
                    });
                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                var responseJson = await response.Content.ReadAsStringAsync();
                                System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                                //    T JsonObject;
                                var JsonObject = JsonConvert.DeserializeObject<T>(responseJson);

                                return Tuple.Create(JsonObject, true, "");

                            }
                            catch (Exception ex)
                            { throw ex; }

                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            LogoutWhenUnAuthorized(response);
                            return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.unAuthoorizaedMsg);
                        }
                        else
                        {
                            //LogoutWhenUnAuthorized(response);
                            return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, response.StatusCode.ToString()); ;
                        }
                    }
                    else
                    {
                        return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.ServerErrorOrNoInternetConnection);
                    }

                }
                else
                {
                    return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.NoInternet);
                }

            }
            catch (System.Exception exp)
            {
                Crashes.TrackError(exp);
                return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, exp.Message);
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }

        }

        public static async Task<Tuple<T, bool, string>> GetAsyncNoHeader<T>(string requestUrl, bool isShowAlert = false) where T : class
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    var client = new System.Net.Http.HttpClient();
                    System.Diagnostics.Debug.WriteLine($"====================== GET: {requestUrl} =====================");
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    var token = "bearer" + " " + Preferences.Get("UserToken", "");
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    //    client.DefaultRequestHeaders.Add("Accept-Language", lang);


                    //System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 3,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");

                        var resultJson = await client.GetAsync(requestUrl);

                        return resultJson;
                    });
                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                            try
                            {
                                var responseJson = await response.Content.ReadAsStringAsync();
                                System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                                //    T JsonObject;
                                var JsonObject = JsonConvert.DeserializeObject<T>(responseJson);

                                return Tuple.Create(JsonObject, true, "");

                            }
                            catch (Exception ex)
                            { throw ex; }

                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            LogoutWhenUnAuthorized(response);
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.unAuthoorizaedMsg);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, response.StatusCode.ToString()); ;
                        }
                    }
                    else
                    {
                        return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.ServerErrorOrNoInternetConnection);
                    }

                }
                else
                {
                    return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, AppResource.NoInternet);
                }

            }
            catch (System.Exception exp)
            {
                Crashes.TrackError(exp);
                return Tuple.Create((T)Activator.CreateInstance(typeof(T)), false, exp.Message);
            }

            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }
        }

        public static async Task<Tuple<string, bool, string>> GetStringAsync(string requestUrl, bool isShowAlert = false)
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    var client = new System.Net.Http.HttpClient();
                    System.Diagnostics.Debug.WriteLine($"======================GET:  {requestUrl} =====================");
                    //client.DefaultRequestHeaders.Clear();
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    var token = "bearer" + " " + Preferences.Get("UserToken", "");
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    client.DefaultRequestHeaders.Add("Accept-Language", lang);

                    System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    System.Diagnostics.Debug.WriteLine($"Authorization: {token}");

                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 3,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");

                        var resultJson = await client.GetAsync(requestUrl);

                        return resultJson;
                    });

                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                            try
                            {
                                var responseJson = await response.Content.ReadAsStringAsync();
                                System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                                //    T JsonObject;
                                if (requestUrl.Contains("TermsAndConditions"))
                                {
                                    //     JsonObject = responseJson;
                                }
                                return Tuple.Create(responseJson, true, "");

                            }
                            catch (Exception ex)
                            {
                                Crashes.TrackError(ex);
                                throw ex;
                            }

                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            LogoutWhenUnAuthorized(response);
                            return Tuple.Create("", false, AppResource.unAuthoorizaedMsg);
                        }
                        else
                        {
                            return Tuple.Create("", false, response.StatusCode.ToString()); ;
                        }
                    }
                    else
                    {
                        return Tuple.Create("", false, AppResource.ServerErrorOrNoInternetConnection);
                    }

                }
                else
                {
                    return Tuple.Create("", false, AppResource.NoInternet);
                }

            }
            catch (System.Exception exp)
            {

                Crashes.TrackError(exp);
                return Tuple.Create("", false, exp.Message);
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }

        }


        private static void LogoutWhenUnAuthorized(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Preferences.Remove("UserLoged");
                    Preferences.Remove("LoginRequestApproved");
                    Preferences.Remove("LoginRequestSent");
                    Application.Current.MainPage = new NavigationPage(new LoginRequest());

                });
            }
        }

        public static async Task<Tuple<bool, string>> GetAsyncString(string requestUrl, bool isShowAlert = false)
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    System.Diagnostics.Debug.WriteLine($"====================== GET: {requestUrl} =====================");
                    client.DefaultRequestHeaders.Clear();
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    var token = "bearer" + " " + Preferences.Get("UserToken", "");
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    client.DefaultRequestHeaders.Add("Accept-Language", lang);
                    System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 3,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");

                        var resultJson = await client.GetAsync(requestUrl);

                        return resultJson;
                    });
                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {

                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return Tuple.Create(true, "");



                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            LogoutWhenUnAuthorized(response);
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return Tuple.Create(false, AppResource.unAuthoorizaedMsg);
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return Tuple.Create(false, AppResource.unAuthoorizaedMsg);
                        }
                        else
                        {
                            return Tuple.Create(false, AppResource.ServerError);

                        }
                    }
                    else
                    {
                        return Tuple.Create(false, AppResource.ServerErrorOrNoInternetConnection);
                    }

                }
                else
                {
                    return Tuple.Create(false, AppResource.NoInternet);
                }

            }
            catch (System.Exception exp)
            {

                Crashes.TrackError(exp);
                return Tuple.Create(false, AppResource.ServerErrorOrNoInternetConnection);
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }

        }


        static string jobject;
        public static async Task<HttpResponseMessage> PostAsync<T>(string requestUrl, T Data, bool isShowAlert = false) where T : class
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    System.Diagnostics.Debug.WriteLine($"======================POST: {requestUrl} =====================");
                    client.DefaultRequestHeaders.Clear();
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                   
                    if (!requestUrl.Contains("Login") && !requestUrl.Contains("ForgotPassword"))
                    {
                        //  client.DefaultRequestHeaders.Add("Authorization", "bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjZCN0FDQzUyMDMwNUJGREI0RjcyNTJEQUVCMjE3N0NDMDkxRkFBRTEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJhM3JNVWdNRnY5dFBjbExhNnlGM3pBa2ZxdUUifQ.eyJuYmYiOjE1ODkxMTYyNDcsImV4cCI6MTU4OTIwMjY0NywiaXNzIjoibnVsbCIsImF1ZCI6WyJudWxsL3Jlc291cmNlcyIsImNhbGxpbmciXSwiY2xpZW50X2lkIjoiY2FsbGluZyIsInN1YiI6ImRiYzY0ZTY4LTFlOWUtNDM4Ny04ZmNjLTJkNjczMmEzMTMwZiIsImF1dGhfdGltZSI6MTU4OTExNjI0NywiaWRwIjoibG9jYWwiLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjRBVk5TRzZITVBTQ1JMU1VOTEw2M1ZDUTQ2VkhPWVQ2Iiwicm9sZSI6IkFnZW50IiwicHJlZmVycmVkX3VzZXJuYW1lIjoib21uaWFEcml2ZXIiLCJuYW1lIjoib21uaWFEcml2ZXIiLCJlbWFpbCI6Im9tbmlhRHJpdmVyQGpqai52diIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwicGhvbmVfbnVtYmVyIjoiMTA5NzAzMDk5OCIsInBob25lX251bWJlcl92ZXJpZmllZCI6ZmFsc2UsIlRlbmFudF9JZCI6IjUxMzg1NDBlLTg2NGYtNDYwNS1iNjFlLTA1MDFjODM4ZDI4OCIsIlVzZXJUeXBlIjoiMCIsInNjb3BlIjpbImNhbGxpbmciXSwiYW1yIjpbInB3ZCJdfQ.jY_4tyLEhcdiAKjkFho2gRZANy_xnaGRZU0em8wReLauNiMI6xUNdiMYsfEOLr7l-qNE9HBXsrR4Bi6fNICPZSJg-rAepMUfRXDZOG9NfNpnQdWZXOLInJo1URUNQ-VVIJRbhALWVvFro2yep_kLj-ChJ_7ybic5BDa9L-9U2ykcb16laP8loomG6bGolEg-YunjRiz99al-mo7eVPh4o0LVqTkOmIbKZD5mrRQpBBVSAKAja1vHH3l-0ddwDF13-DqD16SrpJk1phiz-Gr7hc8phm0Oy1cgYI_4-E-naZZwy3fvRcbZaEfszbcYjKkiyiIlKfIXUGjLIpDPpx9gnQ");
                        var token = Preferences.Get("TokenType", "") + " " + Preferences.Get("UserToken", "");
                        System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                        client.DefaultRequestHeaders.Add("Authorization", token);
                    }

                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    client.DefaultRequestHeaders.Add("Accept-Language", lang);

                    //var JsonObject = JsonConvert.SerializeObject(Data);
                    jobject = JsonConvert.SerializeObject(Data);
                    var JsonObject = jobject;

                    var content = new StringContent(JsonObject, Encoding.UTF8, "application/json");
                    System.Diagnostics.Debug.WriteLine($"============== Body =================");
                    System.Diagnostics.Debug.WriteLine(content);
                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 1,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");

                            var resultJson = await client.PostAsync(requestUrl, content).ConfigureAwait(false);

                            return resultJson;
                        });
                    // var response = await client.PostAsync(requestUrl, content).ConfigureAwait(false) ;
                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return response;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            LogoutWhenUnAuthorized(response);

                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.unAuthoorizaedMsg };
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {

                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = "Internal Server Error" };
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = "Bad Request " + AppResource.ServerError + responseJson };
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                        {

                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = "Bad Gateway " + AppResource.ServerError };
                        }

                        else
                        {
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.ServerError };
                        }
                    }
                    else
                    {
                        return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
                    }

                }
                else
                {
                    return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.NoInternet };
                }

            }
            catch (System.Exception exp)
            {

                Crashes.TrackError(exp);
                return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = "Post.." + requestUrl + exp.Message };
            }
            finally
            {
                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }
            }

        }
        public static async Task<HttpResponseMessage> PostAsync(string requestUrl, bool isShowAlert = false)
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    System.Diagnostics.Debug.WriteLine($"======================POST: {requestUrl} =====================");
                    client.DefaultRequestHeaders.Clear();
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");

                    //  var client = new System.Net.Http.HttpClient();
                    if (!requestUrl.Contains("Login") && !requestUrl.Contains("ForgotPassword"))
                    {
                        //   client.DefaultRequestHeaders.Add("Authorization", "bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjZCN0FDQzUyMDMwNUJGREI0RjcyNTJEQUVCMjE3N0NDMDkxRkFBRTEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJhM3JNVWdNRnY5dFBjbExhNnlGM3pBa2ZxdUUifQ.eyJuYmYiOjE1ODkxMTYyNDcsImV4cCI6MTU4OTIwMjY0NywiaXNzIjoibnVsbCIsImF1ZCI6WyJudWxsL3Jlc291cmNlcyIsImNhbGxpbmciXSwiY2xpZW50X2lkIjoiY2FsbGluZyIsInN1YiI6ImRiYzY0ZTY4LTFlOWUtNDM4Ny04ZmNjLTJkNjczMmEzMTMwZiIsImF1dGhfdGltZSI6MTU4OTExNjI0NywiaWRwIjoibG9jYWwiLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjRBVk5TRzZITVBTQ1JMU1VOTEw2M1ZDUTQ2VkhPWVQ2Iiwicm9sZSI6IkFnZW50IiwicHJlZmVycmVkX3VzZXJuYW1lIjoib21uaWFEcml2ZXIiLCJuYW1lIjoib21uaWFEcml2ZXIiLCJlbWFpbCI6Im9tbmlhRHJpdmVyQGpqai52diIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwicGhvbmVfbnVtYmVyIjoiMTA5NzAzMDk5OCIsInBob25lX251bWJlcl92ZXJpZmllZCI6ZmFsc2UsIlRlbmFudF9JZCI6IjUxMzg1NDBlLTg2NGYtNDYwNS1iNjFlLTA1MDFjODM4ZDI4OCIsIlVzZXJUeXBlIjoiMCIsInNjb3BlIjpbImNhbGxpbmciXSwiYW1yIjpbInB3ZCJdfQ.jY_4tyLEhcdiAKjkFho2gRZANy_xnaGRZU0em8wReLauNiMI6xUNdiMYsfEOLr7l-qNE9HBXsrR4Bi6fNICPZSJg-rAepMUfRXDZOG9NfNpnQdWZXOLInJo1URUNQ-VVIJRbhALWVvFro2yep_kLj-ChJ_7ybic5BDa9L-9U2ykcb16laP8loomG6bGolEg-YunjRiz99al-mo7eVPh4o0LVqTkOmIbKZD5mrRQpBBVSAKAja1vHH3l-0ddwDF13-DqD16SrpJk1phiz-Gr7hc8phm0Oy1cgYI_4-E-naZZwy3fvRcbZaEfszbcYjKkiyiIlKfIXUGjLIpDPpx9gnQ");
                        var token = Preferences.Get("TokenType", "") + " " + Preferences.Get("UserToken", "");
                        System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                        client.DefaultRequestHeaders.Add("Authorization", token);
                    }
                    //var JsonObject = JsonConvert.SerializeObject(Data);
                    //  jobject = JsonConvert.SerializeObject(Data);
                    // var JsonObject = jobject;

                    //    var content = new StringContent(JsonObject, Encoding.UTF8, "application/json");
                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 3,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");

                            var resultJson = await client.PostAsync(requestUrl, null).ConfigureAwait(false);

                            return resultJson;
                        });
                    // var response = await client.PostAsync(requestUrl, content).ConfigureAwait(false) ;

                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return response;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {

                            LogoutWhenUnAuthorized(response);

                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.unAuthoorizaedMsg };
                        }
                        else
                        {
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.ServerError };
                        }
                    }
                    else
                    {
                        return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
                    }

                }
                else
                {
                    return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.NoInternet };
                }

            }
            catch (System.Exception exp)
            {
                Crashes.TrackError(exp);
                return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }

        }

        public static async Task<HttpResponseMessage> PostFormDataAsync(string requestUrl, MultipartFormDataContent contentFormData, bool isShowAlert = false)
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    var client = new System.Net.Http.HttpClient();
                    System.Diagnostics.Debug.WriteLine($"======================POST with Form Data: {requestUrl} =====================");
                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    if (!requestUrl.Contains("Login") && !requestUrl.Contains("ForgotPassword"))
                    {
                        //    //  client.DefaultRequestHeaders.Add("Authorization", "bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjZCN0FDQzUyMDMwNUJGREI0RjcyNTJEQUVCMjE3N0NDMDkxRkFBRTEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJhM3JNVWdNRnY5dFBjbExhNnlGM3pBa2ZxdUUifQ.eyJuYmYiOjE1ODkxMTYyNDcsImV4cCI6MTU4OTIwMjY0NywiaXNzIjoibnVsbCIsImF1ZCI6WyJudWxsL3Jlc291cmNlcyIsImNhbGxpbmciXSwiY2xpZW50X2lkIjoiY2FsbGluZyIsInN1YiI6ImRiYzY0ZTY4LTFlOWUtNDM4Ny04ZmNjLTJkNjczMmEzMTMwZiIsImF1dGhfdGltZSI6MTU4OTExNjI0NywiaWRwIjoibG9jYWwiLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjRBVk5TRzZITVBTQ1JMU1VOTEw2M1ZDUTQ2VkhPWVQ2Iiwicm9sZSI6IkFnZW50IiwicHJlZmVycmVkX3VzZXJuYW1lIjoib21uaWFEcml2ZXIiLCJuYW1lIjoib21uaWFEcml2ZXIiLCJlbWFpbCI6Im9tbmlhRHJpdmVyQGpqai52diIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwicGhvbmVfbnVtYmVyIjoiMTA5NzAzMDk5OCIsInBob25lX251bWJlcl92ZXJpZmllZCI6ZmFsc2UsIlRlbmFudF9JZCI6IjUxMzg1NDBlLTg2NGYtNDYwNS1iNjFlLTA1MDFjODM4ZDI4OCIsIlVzZXJUeXBlIjoiMCIsInNjb3BlIjpbImNhbGxpbmciXSwiYW1yIjpbInB3ZCJdfQ.jY_4tyLEhcdiAKjkFho2gRZANy_xnaGRZU0em8wReLauNiMI6xUNdiMYsfEOLr7l-qNE9HBXsrR4Bi6fNICPZSJg-rAepMUfRXDZOG9NfNpnQdWZXOLInJo1URUNQ-VVIJRbhALWVvFro2yep_kLj-ChJ_7ybic5BDa9L-9U2ykcb16laP8loomG6bGolEg-YunjRiz99al-mo7eVPh4o0LVqTkOmIbKZD5mrRQpBBVSAKAja1vHH3l-0ddwDF13-DqD16SrpJk1phiz-Gr7hc8phm0Oy1cgYI_4-E-naZZwy3fvRcbZaEfszbcYjKkiyiIlKfIXUGjLIpDPpx9gnQ");
                        var token = Preferences.Get("TokenType", "") + " " + Preferences.Get("UserToken", "");
                        System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                    }
                    //var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    //client.DefaultRequestHeaders.Add("Accept-Language", lang);
                    //var JsonObject = JsonConvert.SerializeObject(Data);
                    //   jobject = JsonConvert.SerializeObject(Data);
                    // var JsonObject = jobject;

                    //    var content = new StringContent(JsonObject, Encoding.UTF8, "application/json");
                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await Policy
                    .Handle<HttpRequestException>(ex => !ex.Message.ToLower().Contains("404"))
                    .RetryAsync
                    (
                        retryCount: 3,
                        onRetry: (ex, time) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"Something went wrong: {ex.Message}, retrying...");
                        }
                    )
                    .ExecuteAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to fetch remote data...");
                        System.Diagnostics.Debug.WriteLine($"============== FormData =================");
                        System.Diagnostics.Debug.WriteLine(await contentFormData.ReadAsStringAsync());
                        var resultJson = await client.PostAsync(requestUrl, contentFormData).ConfigureAwait(false);

                        return resultJson;
                    });
                    // var response = await client.PostAsync(requestUrl, content).ConfigureAwait(false) ;
                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return response;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.unAuthoorizaedMsg };
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {

                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = "500 Internal Server Error" };
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = "Bad Request " + AppResource.ServerError + responseJson };
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                        {

                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = "Bad Gateway " + AppResource.ServerError };
                        }

                        else
                        {
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = response.StatusCode.ToString() };
                        }
                    }
                    else
                    {
                        return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
                    }

                }
                else
                {
                    return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.NoInternet };
                }

            }
            catch (System.Exception exp)
            {
                Crashes.TrackError(exp);
                return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }
        }


        public static async Task<HttpResponseMessage> PutAsync<T>(string requestUrl, T Data, bool isShowAlert = false) where T : class
        {
            isShowAlert = false;
            try
            {
                if (NetworkCheck.IsInternet())
                {
                    System.Diagnostics.Debug.WriteLine($"======================PUT: {requestUrl} =====================");
                    client.DefaultRequestHeaders.Clear();

                    System.Diagnostics.Debug.WriteLine($"============== Headers =================");
                    //  var client = new System.Net.Http.HttpClient();
                    //   client.DefaultRequestHeaders.Add("Authorization", app.CurrentToken);
                    var token = Preferences.Get("TokenType", "") + " " + Preferences.Get("UserToken", "");
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    System.Diagnostics.Debug.WriteLine($"Authorization: {token}");
                    var lang = Preferences.Get("LanguageId", "en") == "ar-EG" ? "ar" : Preferences.Get("LanguageId", "en");
                    client.DefaultRequestHeaders.Add("Accept-Language", lang);
                    System.Diagnostics.Debug.WriteLine($"Accept-Language: {lang}");
                    var JsonObject = JsonConvert.SerializeObject(Data);
                    var content = new StringContent(JsonObject, Encoding.UTF8, "application/json");
                    System.Diagnostics.Debug.WriteLine($"============== Body =================");
                    System.Diagnostics.Debug.WriteLine(await content.ReadAsStringAsync());

                    if (isShowAlert)
                        _stopwatch.Start();
                    var response = await client.PutAsync(requestUrl, content);

                    System.Diagnostics.Debug.WriteLine($"============== Response =================");
                    if (response != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"============== {response.ToString()} =================");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"Response: {responseJson}");
                            return response;
                        }
                        else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            LogoutWhenUnAuthorized(response);
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.unAuthoorizaedMsg };
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Response: {response}");
                            return new HttpResponseMessage() { StatusCode = response.StatusCode, ReasonPhrase = AppResource.unAuthoorizaedMsg };
                        }
                    }
                    else
                    {
                        return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
                    }

                }
                else
                {
                    return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.NoInternet };
                }

            }
            catch (System.Exception exp)
            {
                Crashes.TrackError(exp);
                return new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest, ReasonPhrase = AppResource.ServerErrorOrNoInternetConnection };
            }
            finally
            {

                if (isShowAlert)
                {
                    _stopwatch.Stop();
                    TimeSpan ts = _stopwatch.Elapsed;
                    string y = "API Consumption Time" + "M:" + ts.Minutes + "," + "S:" + ts.Seconds + "," + "MS:" + ts.Milliseconds;
                    await _dialogService.DisplayAlertAsync("", y, AppResource.OK);
                    _stopwatch.Reset();
                }

            }
        }

    }
}