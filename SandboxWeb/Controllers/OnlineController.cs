﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.HealthVault.Clients;
using Microsoft.HealthVault.ItemTypes;
using Microsoft.HealthVault.Person;
using Microsoft.HealthVault.Web;
using Microsoft.HealthVault.Web.Attributes;
using Microsoft.HealthVault.Web.Connection;
using NodaTime;

namespace SandboxWeb.Controllers
{
    [RequireSignIn]
    public class OnlineController : Controller
    {
        // GET: HealthVault
        public async Task<ActionResult> Index()
        {
            IWebHealthVaultConnection webHealthVaultConnection = await WebHealthVaultFactory.CreateWebConnectionAsync();

            PersonInfo personInfo = await webHealthVaultConnection.GetPersonInfoAsync();

            IThingClient thingClient = webHealthVaultConnection.CreateThingClient();

            IReadOnlyCollection<Weight> weights = await thingClient.GetThingsAsync<Weight>(personInfo.GetSelfRecord().Id);

            return View(weights);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateWeight()
        {
            IWebHealthVaultConnection webHealthVaultConnection = await WebHealthVaultFactory.CreateWebConnectionAsync();
            PersonInfo personInfo = await webHealthVaultConnection.GetPersonInfoAsync();

            IThingClient thingClient = webHealthVaultConnection.CreateThingClient();

            LocalDateTime nowLocal = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).LocalDateTime;

            await thingClient.CreateNewThingsAsync(personInfo.GetSelfRecord().Id, new List<Weight> { new Weight(new HealthServiceDateTime(nowLocal), new WeightValue(10)) });

            return RedirectToAction("Index", new RouteValueDictionary());
        }
    }
}