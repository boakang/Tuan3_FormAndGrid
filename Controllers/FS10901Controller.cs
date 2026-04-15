using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using HQSoft.Models;

// NOTE:
// Update this namespace, DbContext, and View paths to match your real project.
namespace HQSoft.Controllers
{
    public class FS10901Controller : Controller
    {
        private const string ScreenNbr = "FS10901";

        // TODO: Replace by your real EDMX context class.
        private Product_eSales_2026Entities _db = new Product_eSales_2026Entities();

        // TODO: Replace by your real Current object from framework if available.
        private UserContext Current
        {
            get
            {
                return new UserContext
                {
                    CpnyID = (Session["CpnyID"] ?? string.Empty).ToString(),
                    UserName = (Session["UserName"] ?? User.Identity.Name ?? string.Empty).ToString(),
                    LangID = Convert.ToInt16(Session["LangID"] ?? 0)
                };
            }
        }

        public ActionResult Index()
        {
            // If your project has Util.InitRight(ScreenNbr), call it here.
            return View("~/Views/FS10901/index.cshtml");
        }

        public PartialViewResult Body(string lang)
        {
            return PartialView("~/Views/FS10901/body.cshtml");
        }

        public ActionResult GetFS_Batch_Huy(string branchID, string batchID)
        {
            var data = _db.FS10901_pgBatch_Huy(Current.CpnyID, Current.UserName, Current.LangID, batchID, branchID)
                .Select(x => new
                {
                    x.BatchID,
                    x.CpnyID,
                    OrderDay = x.OrderDay.HasValue ? x.OrderDay.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    x.TotalNumer,
                    x.TotalVolume,
                    x.TotalAmount
                })
                .ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFS_BatchDetail_Huy(string branchID, string batchID)
        {
            var data = _db.FS10901_pgBatchDetail_Huy(Current.CpnyID, Current.UserName, Current.LangID, batchID, branchID).ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(FormCollection data)
        {
            try
            {
                var headerJson = data["header"] ?? "{}";
                var detailsJson = data["details"] ?? "[]";
                var deletedJson = data["deleted"] ?? "[]";

                var curHeader = JsonConvert.DeserializeObject<FS10901_pgBatch_Huy_Result>(headerJson);
                var curDetails = JsonConvert.DeserializeObject<List<FS10901_pgBatchDetail_Huy_Result>>(detailsJson) ?? new List<FS10901_pgBatchDetail_Huy_Result>();
                var deletedDetails = JsonConvert.DeserializeObject<List<FS10901_pgBatchDetail_Huy_Result>>(deletedJson) ?? new List<FS10901_pgBatchDetail_Huy_Result>();

                curDetails = curDetails
                    .Where(p => !string.IsNullOrWhiteSpace(p.InventoryID))
                    .Select(p =>
                    {
                        p.InventoryID = p.InventoryID.Trim();
                        return p;
                    })
                    .ToList();

                if (curHeader == null)
                {
                    return Json(new { success = false, message = "Invalid header" }, JsonRequestBehavior.AllowGet);
                }

                var branchID = (curHeader.CpnyID ?? string.Empty).Trim();
                var batchNbr = (curHeader.BatchID ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(branchID))
                {
                    return Json(new { success = false, message = "CpnyID is required" }, JsonRequestBehavior.AllowGet);
                }

                var duplicatedInventory = curDetails
                    .GroupBy(p => p.InventoryID, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(duplicatedInventory))
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "InventoryID duplicated in detail rows: " + duplicatedInventory
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                if (string.IsNullOrWhiteSpace(batchNbr))
                {
                    batchNbr = GenerateBatchNbr(branchID);
                }

                var batch = _db.FS_Batch_Huy.FirstOrDefault(p => p.CpnyID.ToLower() == branchID.ToLower() && p.BatchID.ToLower() == batchNbr.ToLower());
                if (batch == null)
                {
                    batch = new FS_Batch_Huy();
                    batch.CpnyID = branchID;
                    batch.BatchID = batchNbr;
                    _db.FS_Batch_Huy.AddObject(batch);
                    Update_Batch(batch, curHeader, true);
                }
                else
                {
                    Update_Batch(batch, curHeader, false);
                }

                foreach (var deleted in deletedDetails)
                {
                    var del = _db.FS_BatchDetail_Huy.FirstOrDefault(p =>
                        p.CpnyID.ToLower() == branchID.ToLower() &&
                        p.BatchID.ToLower() == batchNbr.ToLower() &&
                        p.InventoryID.ToLower() == deleted.InventoryID.ToLower());

                    if (del != null)
                    {
                        _db.FS_BatchDetail_Huy.DeleteObject(del);
                    }
                }

                foreach (var cur in curDetails)
                {
                    var det = _db.FS_BatchDetail_Huy.Local.FirstOrDefault(p =>
                                  p.CpnyID.ToLower() == branchID.ToLower() &&
                                  p.BatchID.ToLower() == batchNbr.ToLower() &&
                                  p.InventoryID.ToLower() == cur.InventoryID.ToLower())
                              ?? _db.FS_BatchDetail_Huy.FirstOrDefault(p =>
                                  p.CpnyID.ToLower() == branchID.ToLower() &&
                                  p.BatchID.ToLower() == batchNbr.ToLower() &&
                                  p.InventoryID.ToLower() == cur.InventoryID.ToLower());

                    if (det == null)
                    {
                        det = new FS_BatchDetail_Huy();
                        det.CpnyID = branchID;
                        det.BatchID = batchNbr;
                        det.InventoryID = cur.InventoryID;
                        _db.FS_BatchDetail_Huy.AddObject(det);
                        Update_BatchDetail(det, cur, true);
                    }
                    else
                    {
                        Update_BatchDetail(det, cur, false);
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true, batchNbr = batchNbr }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteData(FormCollection data)
        {
            try
            {
                var branchID = (data["branchID"] ?? string.Empty).Trim();
                var batchID = (data["batchID"] ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(branchID) || string.IsNullOrWhiteSpace(batchID))
                {
                    return Json(new { success = false, message = "BranchID/BatchID is required" }, JsonRequestBehavior.AllowGet);
                }

                var listBatchDetail = _db.FS_BatchDetail_Huy.Where(p => p.CpnyID.ToLower() == branchID.ToLower() && p.BatchID.ToLower() == batchID.ToLower()).ToList();
                if (listBatchDetail != null)
                {
                    foreach (var item in listBatchDetail)
                    {
                        _db.FS_BatchDetail_Huy.DeleteObject(item);
                    }
                }

                var order = _db.FS_Batch_Huy.FirstOrDefault(p => p.CpnyID.ToLower() == branchID.ToLower() && p.BatchID.ToLower() == batchID.ToLower());
                if (order != null)
                {
                    _db.FS_Batch_Huy.DeleteObject(order);
                }

                _db.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, type = "error", errorMsg = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GenerateBatchNbr(string branchID)
        {
            var orderMax = _db.FS_Batch_Huy.Where(p => p.CpnyID.ToLower() == branchID.ToLower()).OrderByDescending(p => p.BatchID).FirstOrDefault();
            if (orderMax == null || string.IsNullOrWhiteSpace(orderMax.BatchID))
            {
                return "IN000001";
            }

            var oldNbr = orderMax.BatchID;
            var numeric = oldNbr.Length > 2 ? oldNbr.Substring(2) : "0";
            int n;
            if (!int.TryParse(numeric, out n))
            {
                n = 0;
            }

            return "IN" + (n + 1).ToString("000000");
        }

        private void Update_Batch(FS_Batch_Huy t, FS10901_pgBatch_Huy_Result s, bool isNew)
        {
            var vnNow = DateTime.UtcNow.AddHours(7);
            var userName = string.IsNullOrWhiteSpace(Current.UserName) ? "SYSTEM" : Current.UserName;

            t.OrderDay = s.OrderDay ?? vnNow;
            t.TotalNumer = s.TotalNumer;
            t.TotalVolume = s.TotalVolume;
            t.TotalAmount = s.TotalAmount;

            if (isNew)
            {
                t.Crtd_DateTime = vnNow;
                t.Crtd_Prog = ScreenNbr;
                t.Crtd_User = userName;
            }

            t.LUpd_DateTime = vnNow;
            t.LUpd_Prog = ScreenNbr;
            t.LUpd_User = userName;
        }

        private void Update_BatchDetail(FS_BatchDetail_Huy t, FS10901_pgBatchDetail_Huy_Result s, bool isNew)
        {
            var vnNow = DateTime.UtcNow.AddHours(7);
            var userName = string.IsNullOrWhiteSpace(Current.UserName) ? "SYSTEM" : Current.UserName;

            t.Number = s.Number;
            t.Volume = s.Volume;
            t.Price = s.Price;
            t.Tax = s.Tax;
            t.Amount = s.Amount;

            if (isNew)
            {
                t.Crtd_DateTime = vnNow;
                t.Crtd_Prog = ScreenNbr;
                t.Crtd_User = userName;
            }

            t.LUpd_DateTime = vnNow;
            t.LUpd_Prog = ScreenNbr;
            t.LUpd_User = userName;
        }
    }

    public class UserContext
    {
        public string CpnyID { get; set; }
        public string UserName { get; set; }
        public short LangID { get; set; }
    }
}
