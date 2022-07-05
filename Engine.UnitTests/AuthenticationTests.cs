﻿using Newtonsoft.Json;
using NUnit.Framework;
using OpenTap.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenTap.UnitTests
{
    public class AuthenticationTests
    {
        [Test]
        public void ParseTokens()
        {
            string response = @"{
    ""access_token"": ""eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJIaHlId25IdDRnclRDYVhtRHNlSVhHX2U3ajVNb3YzakhLTjZWVlZsZ0lNIn0.eyJleHAiOjE2NDg3MTEwMDQsImlhdCI6MTY0ODcxMDcwNCwianRpIjoiM2NkZDRkYzEtMGE2Mi00YzBjLTljNzQtMmFhZTUwNDk2YWM1IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5rczg1MDAuYWxiLmlzLmtleXNpZ2h0LmNvbS9hdXRoL3JlYWxtcy9rczg1MDAiLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiMWY1NmRkMGItNzFkOS00MjAwLWI0YzYtZDE5NWNlNGUxYzJiIiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZGVubmlzIiwic2Vzc2lvbl9zdGF0ZSI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1rczg1MDAiLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCIsInNpZCI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImNsaWVudElkIjoiZGVubmlzIiwiY2xpZW50SG9zdCI6IjEwLjE0OS4xMDkuMjUyIiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXJ2aWNlLWFjY291bnQtZGVubmlzIiwiY2xpZW50QWRkcmVzcyI6IjEwLjE0OS4xMDkuMjUyIn0.GWKY9EV4sqpLg0GGfmqnGcfjBGhN2NunJrfIysaeRJaYnfsmo3rt-_awpbg15q6IXFipr8N6kE965Y0rxeODxAmRVIf8pb-GkaT0qMOpUidiZrUz3FC0WDXymH3gBayOaKOIa03qVOn5fURmGV4nbQyuJemgQYXW8fcFQpu8xrsM9leYGzVXU4zdxNR-jSfYq1iNN2je9E-EhlglxmvQnirRcoGJsymLxg0s6M_s6cQnQBOihuKsEPE8C3zBeVoCXYJ3kkY0Q6GGK2e8EoRhvrNQ-pyK58yJv5YEnC6Erxe06tfFBZ_XX596YerAqkI4XlHpuEBddqGP0HSYlwtcjA"",
    ""expires_in"": 300,
    ""refresh_expires_in"": 1800,
    ""refresh_token"": ""eyJhbGciOiJIUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJmMjk4MTY1ZS01MDE5LTQ2YjQtYTYyNS1hMzhiMGY0MzUwZDgifQ.eyJleHAiOjE2NDg3MTI1MDQsImlhdCI6MTY0ODcxMDcwNCwianRpIjoiOTdhOGE3YmEtMTU0OS00NWU5LTgxN2YtY2E1M2YyNzkzYjcwIiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5rczg1MDAuYWxiLmlzLmtleXNpZ2h0LmNvbS9hdXRoL3JlYWxtcy9rczg1MDAiLCJhdWQiOiJodHRwczovL2tleWNsb2FrLmtzODUwMC5hbGIuaXMua2V5c2lnaHQuY29tL2F1dGgvcmVhbG1zL2tzODUwMCIsInN1YiI6IjFmNTZkZDBiLTcxZDktNDIwMC1iNGM2LWQxOTVjZTRlMWMyYiIsInR5cCI6IlJlZnJlc2giLCJhenAiOiJkZW5uaXMiLCJzZXNzaW9uX3N0YXRlIjoiMDVkN2E2ZjItNzBkZC00OGUzLTgwNmItZGM5Y2NiNWU3ZjNlIiwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCIsInNpZCI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSJ9.VftS1sPVoP_vrYBRuOy7ZT-J9L8SCofxUH1L_5pI6FU"",
    ""token_type"": ""Bearer"",
    ""id_token"": ""eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJIaHlId25IdDRnclRDYVhtRHNlSVhHX2U3ajVNb3YzakhLTjZWVlZsZ0lNIn0.eyJleHAiOjE2NDg3MTEwMDQsImlhdCI6MTY0ODcxMDcwNCwiYXV0aF90aW1lIjowLCJqdGkiOiIwNDQ1MjJmZi1iZWZmLTQxOWYtOTcyNC0yNWJmMDY5ZTdjMjgiLCJpc3MiOiJodHRwczovL2tleWNsb2FrLmtzODUwMC5hbGIuaXMua2V5c2lnaHQuY29tL2F1dGgvcmVhbG1zL2tzODUwMCIsImF1ZCI6ImRlbm5pcyIsInN1YiI6IjFmNTZkZDBiLTcxZDktNDIwMC1iNGM2LWQxOTVjZTRlMWMyYiIsInR5cCI6IklEIiwiYXpwIjoiZGVubmlzIiwic2Vzc2lvbl9zdGF0ZSI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImF0X2hhc2giOiJvMElRSnRyYW52NjA0RUFuWVFETUVnIiwiYWNyIjoiMSIsInNpZCI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImNsaWVudElkIjoiZGVubmlzIiwiY2xpZW50SG9zdCI6IjEwLjE0OS4xMDkuMjUyIiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXJ2aWNlLWFjY291bnQtZGVubmlzIiwiY2xpZW50QWRkcmVzcyI6IjEwLjE0OS4xMDkuMjUyIn0.JgqiDXT0aArwz0P6nbN8c6HPuVsmUHPuXeCFEsTjf3VdN0PjX2j8thrkGmBw6dr5bSwpX0LTP1vxZfspIe-rpi5UuiKCAmkY92M8T5A7m3yI8gDvlA3RzjXAnrTu3it436444YpYwV9PlQZipy7pIaaWqDOP4AJbnhGARWNHTMozSBCClmQva50nzjEGFFI4Z2ZI9-SPbETdY1xxAqept7JMLnJuPRw_BXFQc8oYTGnqr7kBm8mQnWoFbFi1DZk7VP7e0sSYaI9H3SWZgc2jNpiAa4tpDx9GnTVV4mtQjxB2Xvl_KwZirszewdnDo83M4TnV79PzIr3aJAjOD7crDw"",
    ""not-before-policy"": 0,
    ""session_state"": ""05d7a6f2-70dd-48e3-806b-dc9ccb5e7f3e"",
    ""scope"": ""openid profile email""
}";
            var tokens = TokenInfo.ParseTokens(response, "http://packages.opentap.io");
            Assert.AreEqual(3, tokens.Count);


            response = @"{
    ""access_token"": ""eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJIaHlId25IdDRnclRDYVhtRHNlSVhHX2U3ajVNb3YzakhLTjZWVlZsZ0lNIn0.eyJleHAiOjE2NDg3MTEwMDQsImlhdCI6MTY0ODcxMDcwNCwianRpIjoiM2NkZDRkYzEtMGE2Mi00YzBjLTljNzQtMmFhZTUwNDk2YWM1IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5rczg1MDAuYWxiLmlzLmtleXNpZ2h0LmNvbS9hdXRoL3JlYWxtcy9rczg1MDAiLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiMWY1NmRkMGItNzFkOS00MjAwLWI0YzYtZDE5NWNlNGUxYzJiIiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZGVubmlzIiwic2Vzc2lvbl9zdGF0ZSI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1rczg1MDAiLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCIsInNpZCI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImNsaWVudElkIjoiZGVubmlzIiwiY2xpZW50SG9zdCI6IjEwLjE0OS4xMDkuMjUyIiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXJ2aWNlLWFjY291bnQtZGVubmlzIiwiY2xpZW50QWRkcmVzcyI6IjEwLjE0OS4xMDkuMjUyIn0.GWKY9EV4sqpLg0GGfmqnGcfjBGhN2NunJrfIysaeRJaYnfsmo3rt-_awpbg15q6IXFipr8N6kE965Y0rxeODxAmRVIf8pb-GkaT0qMOpUidiZrUz3FC0WDXymH3gBayOaKOIa03qVOn5fURmGV4nbQyuJemgQYXW8fcFQpu8xrsM9leYGzVXU4zdxNR-jSfYq1iNN2je9E-EhlglxmvQnirRcoGJsymLxg0s6M_s6cQnQBOihuKsEPE8C3zBeVoCXYJ3kkY0Q6GGK2e8EoRhvrNQ-pyK58yJv5YEnC6Erxe06tfFBZ_XX596YerAqkI4XlHpuEBddqGP0HSYlwtcjA"",
    ""expires_in"": 300,
    ""refresh_expires_in"": 1800,
    ""refresh_token"": ""eyJhbGciOiJIUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJmMjk4MTY1ZS01MDE5LTQ2YjQtYTYyNS1hMzhiMGY0MzUwZDgifQ.eyJleHAiOjE2NDg3MTI1MDQsImlhdCI6MTY0ODcxMDcwNCwianRpIjoiOTdhOGE3YmEtMTU0OS00NWU5LTgxN2YtY2E1M2YyNzkzYjcwIiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5rczg1MDAuYWxiLmlzLmtleXNpZ2h0LmNvbS9hdXRoL3JlYWxtcy9rczg1MDAiLCJhdWQiOiJodHRwczovL2tleWNsb2FrLmtzODUwMC5hbGIuaXMua2V5c2lnaHQuY29tL2F1dGgvcmVhbG1zL2tzODUwMCIsInN1YiI6IjFmNTZkZDBiLTcxZDktNDIwMC1iNGM2LWQxOTVjZTRlMWMyYiIsInR5cCI6IlJlZnJlc2giLCJhenAiOiJkZW5uaXMiLCJzZXNzaW9uX3N0YXRlIjoiMDVkN2E2ZjItNzBkZC00OGUzLTgwNmItZGM5Y2NiNWU3ZjNlIiwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCIsInNpZCI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSJ9.VftS1sPVoP_vrYBRuOy7ZT-J9L8SCofxUH1L_5pI6FU"",
    ""token_type"": ""Bearer"",
    ""not-before-policy"": 0,
    ""session_state"": ""05d7a6f2-70dd-48e3-806b-dc9ccb5e7f3e"",
    ""scope"": ""openid profile email""
}";
            tokens = TokenInfo.ParseTokens(response, "http://packages.opentap.io");
            Assert.AreEqual(2, tokens.Count);

            response = @"{
    ""access_token"": ""eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJIaHlId25IdDRnclRDYVhtRHNlSVhHX2U3ajVNb3YzakhLTjZWVlZsZ0lNIn0.eyJleHAiOjE2NDg3MTEwMDQsImlhdCI6MTY0ODcxMDcwNCwianRpIjoiM2NkZDRkYzEtMGE2Mi00YzBjLTljNzQtMmFhZTUwNDk2YWM1IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5rczg1MDAuYWxiLmlzLmtleXNpZ2h0LmNvbS9hdXRoL3JlYWxtcy9rczg1MDAiLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiMWY1NmRkMGItNzFkOS00MjAwLWI0YzYtZDE5NWNlNGUxYzJiIiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZGVubmlzIiwic2Vzc2lvbl9zdGF0ZSI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1rczg1MDAiLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJvcGVuaWQgcHJvZmlsZSBlbWFpbCIsInNpZCI6IjA1ZDdhNmYyLTcwZGQtNDhlMy04MDZiLWRjOWNjYjVlN2YzZSIsImNsaWVudElkIjoiZGVubmlzIiwiY2xpZW50SG9zdCI6IjEwLjE0OS4xMDkuMjUyIiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXJ2aWNlLWFjY291bnQtZGVubmlzIiwiY2xpZW50QWRkcmVzcyI6IjEwLjE0OS4xMDkuMjUyIn0.GWKY9EV4sqpLg0GGfmqnGcfjBGhN2NunJrfIysaeRJaYnfsmo3rt-_awpbg15q6IXFipr8N6kE965Y0rxeODxAmRVIf8pb-GkaT0qMOpUidiZrUz3FC0WDXymH3gBayOaKOIa03qVOn5fURmGV4nbQyuJemgQYXW8fcFQpu8xrsM9leYGzVXU4zdxNR-jSfYq1iNN2je9E-EhlglxmvQnirRcoGJsymLxg0s6M_s6cQnQBOihuKsEPE8C3zBeVoCXYJ3kkY0Q6GGK2e8EoRhvrNQ-pyK58yJv5YEnC6Erxe06tfFBZ_XX596YerAqkI4XlHpuEBddqGP0HSYlwtcjA"",
    ""expires_in"": 300,
    ""token_type"": ""Bearer"",
    ""not-before-policy"": 0,
    ""session_state"": ""05d7a6f2-70dd-48e3-806b-dc9ccb5e7f3e"",
    ""scope"": ""openid profile email""
}";
            tokens = TokenInfo.ParseTokens(response, "http://packages.opentap.io");
            Assert.AreEqual(1, tokens.Count);
        }

        [Test]
        public void RelativeUrl()
        {
            string host = "https://ks8500.alb.is.keysight.com/";
            // REST-API
            AuthenticationSettings.Current.BaseAddress = host;


            // Plugin in REST-API Process
            HttpClient client = AuthenticationSettings.Current.GetClient();
            Assert.AreEqual(host, client.BaseAddress.ToString());

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/packages/3.0/GetPackageNames");
            requestMessage.Headers.Accept.Clear();
            requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.SendAsync(requestMessage).GetAwaiter().GetResult();
            List<string> packageNames = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            Assert.IsTrue(packageNames.Any());
        }
    }
}
