﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSOCore.ApiLogic;

namespace MSOWeb.Controllers
{
    [Authorize(Roles="Superadmin, Admin")]
    public class OlympiadController : Controller
    {
        public ActionResult Index()
        {
            var logic = new OlympiadsLogic();
            var model = logic.GetOlympiads();

            return View(model);
        }

        [HttpGet]
        public ActionResult Olympiad(int id)
        {
            var logic = new OlympiadsLogic();
            var model = logic.GetOlympiad(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Olympiad(/* form*/)
        {
            // post to update
            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult Event(int id)
        {
            var logic = new OlympiadsLogic();
            var model = logic.GetEvent(id);

            return View(model);
        }
    }
}
