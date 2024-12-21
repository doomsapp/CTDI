using CandidateProject.EntityModels;
using CandidateProject.ViewModels;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Web.Mvc;

namespace CandidateProject.Controllers
{
    public class CartonController : Controller
    {
        private CartonContext db = new CartonContext();

        // GET: Carton
        public ActionResult Index()
        {
            var cartons = db.Cartons
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .ToList();

            return View(cartons);
        }

        // GET: Carton/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // GET: Carton/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Carton/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CartonNumber")] Carton carton)
        {
            if (ModelState.IsValid)
            {
                db.Cartons.Add(carton);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carton);
        }

        // GET: Carton/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonViewModel()
                {
                    Id = c.Id,
                    CartonNumber = c.CartonNumber
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CartonNumber")] CartonViewModel cartonViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons.Find(cartonViewModel.Id);
                carton.CartonNumber = cartonViewModel.CartonNumber;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cartonViewModel);
        }

        // GET: Carton/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carton carton = db.Cartons.Find(id);
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }

        // POST: Carton/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Carton carton = db.Cartons.Find(id);
            db.Cartons.Remove(carton);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult AddEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id
                })
                .SingleOrDefault();

            if (carton == null)
            {
                return HttpNotFound();
            }

            var equipment = db.Equipments
                .Where(e => !db.CartonDetails.Where(cd => cd.CartonId == id).Select(cd => cd.EquipmentId).Contains(e.Id) )
                .Select(e => new EquipmentViewModel()
                {
                    Id = e.Id,
                    ModelType = e.ModelType.TypeName,
                    SerialNumber = e.SerialNumber
                })
                .ToList();
            
            carton.Equipment = equipment;
            return View(carton);
        }

        public ActionResult AddEquipmentToCarton([Bind(Include = "CartonId,EquipmentId")] AddEquipmentViewModel addEquipmentViewModel)
        {
            if (ModelState.IsValid)
            {
                var carton = db.Cartons
                    .Include(c => c.CartonDetails)
                    .Where(c => c.Id == addEquipmentViewModel.CartonId)
                    .SingleOrDefault();
                if (carton == null)
                {
                    return HttpNotFound();
                }
                var equipment = db.Equipments
                    .Where(e => e.Id == addEquipmentViewModel.EquipmentId)
                    .SingleOrDefault();
                if (equipment == null)
                {
                    return HttpNotFound();
                }
                var detail = new CartonDetail()
                {
                    Carton = carton,
                    Equipment = equipment
                };
                carton.CartonDetails.Add(detail);
                db.SaveChanges();
            }
            return RedirectToAction("AddEquipment", new { id = addEquipmentViewModel.CartonId });
        }

        public ActionResult ViewCartonEquipment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var carton = db.Cartons
                .Where(c => c.Id == id)
                .Select(c => new CartonDetailsViewModel()
                {
                    CartonNumber = c.CartonNumber,
                    CartonId = c.Id,
                    Equipment = c.CartonDetails
                        .Select(cd => new EquipmentViewModel()
                        {
                            Id = cd.EquipmentId,
                            ModelType = cd.Equipment.ModelType.TypeName,
                            SerialNumber = cd.Equipment.SerialNumber
                        })
                })
                .SingleOrDefault();
            if (carton == null)
            {
                return HttpNotFound();
            }
            return View(carton);
        }


        //1-Implement the RemoveEquipmentOnCarton action on the CartonController, right now it is just throwing a BadRequest.
        [HttpPost]
        public ActionResult RemoveEquipmentOnCarton([Bind(Include = "CartonId,EquipmentId")] RemoveEquipmentViewModel removeEquipmentViewModel)
        {

            var cartonDetail = db.CartonDetails.FirstOrDefault(cd => cd.CartonId == removeEquipmentViewModel.CartonId && cd.EquipmentId == removeEquipmentViewModel.EquipmentId);

            if (cartonDetail == null)
                return BadRequest("Equipment not found in this carton.");

            db.CartonDetails.Remove(cartonDetail);
            db.SaveChanges();

            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //if (ModelState.IsValid)
            //{
            //    //Remove code here
            //}
            return RedirectToAction("ViewCartonEquipment", new { id = removeEquipmentViewModel.CartonId });
        }

        //Bugs reported by the customers
        //1- We can delete empty cartons from the system, but cannot delete cartons that have items. We should be able to delete a carton at any time.
        [HttpPost]
        public ActionResult DeleteCarton(int cartonId)
        {
            var cartonDetails = db.CartonDetails.Where(cd => cd.CartonId == cartonId).ToList();

            if (cartonDetails.Any())
            {
                db.CartonDetails.RemoveRange(cartonDetails);
            }

            var carton = db.Cartons.FirstOrDefault(c => c.Id == cartonId);

            if (carton == null)
                return BadRequest("Carton not found.");

            db.Cartons.Remove(carton);
            db.SaveChanges();

            return Json(new { success = true, message = "Carton deleted successfully." });
        }

        //2-We can add the same piece of equipment to 2 different cartons, this doesn’t make sense. We should only be able to add a piece of equipment to 1 carton, once a piece of equipment is on a carton it should be unavailable to place on another carton.
        [HttpPost]
        public ActionResult AddEquipmentToCarton(int cartonId, int equipmentId)
        {
            var carton = db.Cartons.FirstOrDefault(c => c.Id == cartonId);
            var equipment = db.Equipments.FirstOrDefault(e => e.Id == equipmentId);

            if (carton == null || equipment == null)
                return BadRequest("Carton or equipment not found.");

            if (db.CartonDetails.Count(cd => cd.CartonId == cartonId) >= 10)
                return BadRequest("Carton is full. Maximum capacity is 10.");
            
            if (db.CartonDetails.Any(cd => cd.EquipmentId == equipmentId))
                return BadRequest("Equipment is already in another carton.");

            var cartonDetail = new CartonDetail
            {
                CartonId = cartonId,
                EquipmentId = equipmentId
            };

            db.CartonDetails.Add(cartonDetail);
            db.SaveChanges();

            return Json(new { success = true, message = "Equipment added to carton." });
        }

        [HttpPost]
        public ActionResult RemoveAllCartonItems(int cartonId)
        {
            var cartonDetails = db.CartonDetails.Where(cd => cd.CartonId == cartonId).ToList();

            if (!cartonDetails.Any())
                return BadRequest("Carton is already empty.");

            db.CartonDetails.RemoveRange(cartonDetails);
            db.SaveChanges();

            return Json(new { success = true, message = "All items removed from the carton." });
        }

        // Helper method for bad requestsbadre
        private ActionResult BadRequest(string message)
        {
            Response.StatusCode = 400;
            return Json(new { success = false, message });
        }


    }
}
