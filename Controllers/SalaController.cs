using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanificatorSali.Data;
using PlanificatorSali.Models;
using SQLitePCL;

namespace PlanificatorSali.Controllers
{
    public class SalaController : Controller
    {
        private readonly PlanificatorSaliContext _context;

        public SalaController(PlanificatorSaliContext context)
        {
            _context = context;
        }

       // GET: Sala
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sala.ToListAsync());
        }

        /* // GET: Sala/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sala = await _context.Sala
                .FirstOrDefaultAsync(m => m.salaID == id);
            if (sala == null)
            {
                return NotFound();
            }

            return View(sala);
        }

        

        private bool SalaExists(int id)
        {
            return _context.Sala.Any(e => e.salaID == id);
        }*/

        [HttpPost]
        public IActionResult Create(string denumire, int etaj, int capacitate, string descriere)
        {
            var sala = new Sala
            { Denumire=denumire,
            capacitate=capacitate,
            Etaj=etaj,
            infosala=descriere};
            _context.Sala.Add(sala);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var sala = _context.Sala.Find(id);
            _context.Sala.Remove(sala);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]

        public IActionResult Find(int id)
        {
            var sala = _context.Sala.Find(id);
            return new JsonResult(sala);
        }

        [HttpPost]
        public IActionResult Update(int salaID, string denumire, int etaj, int capacitate, string descriere)
        {
            var sala = _context.Sala.Find(salaID);
            sala.Denumire = denumire;
            sala.Etaj = etaj;
            sala.capacitate = capacitate;
            sala.infosala = descriere;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}
