(function () {
    var deleted = [];
    var comboData = {
        cpnyList: [],
        batchList: [],
        inventoryList: []
    };

    function id(name) {
        return document.getElementById(name);
    }

    function status(msg, isError) {
        var el = id("status");
        if (!el) {
            return;
        }
        el.style.color = isError ? "#b91c1c" : "#065f46";
        el.textContent = msg;
    }

    function toNumber(v) {
        var n = parseFloat(v);
        return isNaN(n) ? 0 : n;
    }

    function formUrlEncoded(obj) {
        return Object.keys(obj)
            .map(function (k) { return encodeURIComponent(k) + "=" + encodeURIComponent(obj[k]); })
            .join("&");
    }

    function getVietnamTodayDateValue() {
        var vnMillis = Date.now() + (7 * 60 * 60 * 1000);
        var vn = new Date(vnMillis);
        var y = vn.getUTCFullYear();
        var m = String(vn.getUTCMonth() + 1).padStart(2, "0");
        var d = String(vn.getUTCDate()).padStart(2, "0");
        return y + "-" + m + "-" + d;
    }

    function applyOrderDayMinConstraint() {
        var el = id("orderDay");
        if (!el) {
            return;
        }
        el.min = getVietnamTodayDateValue();
    }

    function isOrderDayBeforeToday(orderDayValue) {
        if (!orderDayValue) {
            return false;
        }
        return orderDayValue < getVietnamTodayDateValue();
    }

    function buildInventoryOptionsHtml(selectedInventoryID) {
        var html = "<option value=''></option>";
        comboData.inventoryList.forEach(function (inv) {
            var selected = (selectedInventoryID && inv.InventoryID === selectedInventoryID) ? " selected" : "";
            html += "<option value='" + inv.InventoryID + "'" + selected + ">" + inv.InventoryID + " - " + inv.InventoryName + "</option>";
        });
        return html;
    }

    function refreshInventorySelects() {
        var selects = document.querySelectorAll("#detailBody .inventory");
        selects.forEach(function (sel) {
            var currentValue = (sel.value || "").trim();
            sel.innerHTML = buildInventoryOptionsHtml(currentValue);
            if (currentValue && !comboData.inventoryList.some(function (x) { return x.InventoryID === currentValue; })) {
                sel.value = "";
            }
        });
    }

    function loadComboData() {
        Promise.all([
            fetch("/FS10901/GetCpnyList").then(function (r) { return r.json(); }),
            fetch("/FS10901/GetInventoryList").then(function (r) { return r.json(); })
        ]).then(function (res) {
            comboData.cpnyList = res[0].data || [];
            comboData.inventoryList = res[1].data || [];
            populateBranchSelect();
            refreshInventorySelects();
        }).catch(function (err) {
            console.error("Load combo failed:", err);
        });
    }

    function populateBranchSelect() {
        var sel = id("branchID");
        if (!sel) return;

        var currentValue = sel.value;
        sel.innerHTML = "";

        comboData.cpnyList.forEach(function (item) {
            var opt = document.createElement("option");
            opt.value = item.CpnyID;
            opt.text = item.CpnyID + " - " + item.CpnyName;
            sel.appendChild(opt);
        });

        if (currentValue && comboData.cpnyList.find(function (c) { return c.CpnyID === currentValue; })) {
            sel.value = currentValue;
        } else if (comboData.cpnyList.length > 0) {
            sel.value = comboData.cpnyList[0].CpnyID;
        }

        if (sel.value) {
            populateBatchSelect(sel.value);
        }
    }

    function populateBatchSelect(branchID) {
        var sel = id("batchID");
        if (!sel) return;

        if (!branchID) {
            sel.innerHTML = "<option value=\"\">Để trống để tự sinh</option>";
            return;
        }

        fetch("/FS10901/GetBatchList?branchID=" + encodeURIComponent(branchID))
            .then(function (r) { return r.json(); })
            .then(function (res) {
                var batchList = res.data || [];
                sel.innerHTML = "<option value=\"\">Để trống để tự sinh</option>";

                batchList.forEach(function (item) {
                    var opt = document.createElement("option");
                    opt.value = item.BatchID;
                    opt.text = item.BatchID + " (" + item.OrderDay + ")";
                    sel.appendChild(opt);
                });
            })
            .catch(function (err) {
                console.error("Load batch list failed:", err);
            });
    }

    function ensureBatchOption(batchID, orderDay) {
        var sel = id("batchID");
        if (!sel || !batchID) {
            return;
        }

        var exists = Array.prototype.some.call(sel.options, function (opt) {
            return opt.value === batchID;
        });

        if (!exists) {
            var opt = document.createElement("option");
            opt.value = batchID;
            opt.text = orderDay ? (batchID + " (" + orderDay + ")") : batchID;
            sel.appendChild(opt);
        }

        sel.value = batchID;
    }

    function setTodayIfEmpty() {
        var d = id("orderDay");
        if (d && !d.value) {
            d.value = getVietnamTodayDateValue();
        }
    }

    function toDateInputValue(raw) {
        if (!raw) {
            return "";
        }

        var s = String(raw).trim();

        var dateMatch = s.match(/\d{4}-\d{2}-\d{2}/);
        if (dateMatch) {
            return dateMatch[0];
        }

        var dotNetDate = s.match(/\/Date\(([-]?\d+)(?:[+-]\d{4})?\)\//);
        if (dotNetDate) {
            var ticks = parseInt(dotNetDate[1], 10);
            if (!isNaN(ticks)) {
                var utc = new Date(ticks);
                var uy = utc.getUTCFullYear();
                var um = String(utc.getUTCMonth() + 1).padStart(2, "0");
                var ud = String(utc.getUTCDate()).padStart(2, "0");
                return uy + "-" + um + "-" + ud;
            }
        }

        var tIndex = s.indexOf("T");
        if (tIndex > 0) {
            return s.slice(0, tIndex);
        }

        var spaceIndex = s.indexOf(" ");
        if (spaceIndex > 0) {
            return s.slice(0, spaceIndex);
        }

        if (/^\d{4}-\d{2}-\d{2}$/.test(s)) {
            return s;
        }

        var dt = new Date(s);
        if (isNaN(dt.getTime())) {
            return "";
        }

        var y = dt.getFullYear();
        var m = String(dt.getMonth() + 1).padStart(2, "0");
        var d = String(dt.getDate()).padStart(2, "0");
        return y + "-" + m + "-" + d;
    }

    function recalcRow(tr) {
        var number = toNumber(tr.querySelector(".number").value);
        var volume = toNumber(tr.querySelector(".volume").value);
        var price = toNumber(tr.querySelector(".price").value);
        var tax = toNumber(tr.querySelector(".tax").value);
        var taxPrice = volume * number * tax * price / 100;
        var amount = volume * number * price + taxPrice;
        tr.querySelector(".amount").value = amount.toFixed(2);
    }

    function recalcTotal() {
        var rows = document.querySelectorAll("#detailBody tr");
        var totalNumber = 0;
        var totalVolume = 0;
        var totalAmount = 0;

        rows.forEach(function (tr) {
            totalNumber += toNumber(tr.querySelector(".number").value);
            totalVolume += toNumber(tr.querySelector(".volume").value);
            totalAmount += toNumber(tr.querySelector(".amount").value);
        });

        id("totalNumber").value = totalNumber.toFixed(2);
        id("totalVolume").value = totalVolume.toFixed(2);
        id("totalAmount").value = totalAmount.toFixed(2);
    }

    function bindRow(tr) {
        [".number", ".volume", ".price", ".tax"].forEach(function (css) {
            tr.querySelector(css).addEventListener("input", function () {
                recalcRow(tr);
                recalcTotal();
            });
        });

        tr.querySelector(".btnDel").addEventListener("click", function () {
            var inv = tr.querySelector(".inventory").value.trim();
            if (inv) {
                deleted.push({
                    CpnyID: id("branchID").value.trim(),
                    BatchID: id("batchID").value.trim(),
                    InventoryID: inv
                });
            }
            tr.remove();
            recalcTotal();
        });
    }

    function appendRow(item) {
        var tr = document.createElement("tr");
        var inventorySelect = "<select class='inventory'>" + buildInventoryOptionsHtml(item.InventoryID) + "</select>";

        tr.innerHTML = ""
            + "<td>" + inventorySelect + "</td>"
            + "<td><input class='number' type='number' min='0' step='1' value='" + toNumber(item.Number).toFixed(0) + "'></td>"
            + "<td><input class='volume' type='number' min='0' step='0.01' value='" + toNumber(item.Volume).toFixed(2) + "'></td>"
            + "<td><input class='price' type='number' min='0' step='0.01' value='" + toNumber(item.Price).toFixed(2) + "'></td>"
            + "<td><input class='tax' type='number' min='0' step='0.1' value='" + toNumber(item.Tax).toFixed(1) + "'></td>"
            + "<td><input class='amount' type='number' readonly value='" + toNumber(item.Amount).toFixed(2) + "'></td>"
            + "<td><button type='button' class='btnDel danger'>X</button></td>";

        id("detailBody").appendChild(tr);
        bindRow(tr);
        recalcRow(tr);
        recalcTotal();
    }

    function getHeaderFromForm() {
        var orderDay = id("orderDay").value;
        return {
            CpnyID: id("branchID").value.trim(),
            BatchID: id("batchID").value.trim(),
            OrderDay: orderDay || null,
            TotalNumer: toNumber(id("totalNumber").value),
            TotalVolume: toNumber(id("totalVolume").value),
            TotalAmount: toNumber(id("totalAmount").value)
        };
    }

    function getDetailsFromGrid() {
        var rows = document.querySelectorAll("#detailBody tr");
        var branch = id("branchID").value.trim();
        var batch = id("batchID").value.trim();
        var list = [];

        rows.forEach(function (tr) {
            var inv = tr.querySelector(".inventory").value.trim();
            if (!inv) {
                return;
            }

            list.push({
                CpnyID: branch,
                BatchID: batch,
                InventoryID: inv,
                Number: toNumber(tr.querySelector(".number").value),
                Volume: toNumber(tr.querySelector(".volume").value),
                Price: toNumber(tr.querySelector(".price").value),
                Tax: toNumber(tr.querySelector(".tax").value),
                Amount: toNumber(tr.querySelector(".amount").value)
            });
        });

        return list;
    }

    function findDuplicateInventory(details) {
        var seen = {};
        for (var i = 0; i < details.length; i++) {
            var inv = (details[i].InventoryID || "").trim();
            if (!inv) {
                continue;
            }

            var key = inv.toUpperCase();
            if (seen[key]) {
                return inv;
            }
            seen[key] = true;
        }
        return "";
    }

    function request(url, params) {
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
            },
            body: formUrlEncoded(params || {})
        }).then(function (r) { return r.json(); });
    }

    function loadData() {
        var branch = id("branchID").value.trim();
        var batch = id("batchID").value.trim();

        if (!branch) {
            status("BranchID không được để trống.", true);
            return;
        }

        if (!batch) {
            status("Vui lòng chọn hoặc nhập BatchID trước khi load.", true);
            return;
        }

        status("Đang load dữ liệu...", false);

        Promise.all([
            fetch("/FS10901/GetFS_Batch_Huy?branchID=" + encodeURIComponent(branch) + "&batchID=" + encodeURIComponent(batch)).then(function (r) { return r.json(); }),
            fetch("/FS10901/GetFS_BatchDetail_Huy?branchID=" + encodeURIComponent(branch) + "&batchID=" + encodeURIComponent(batch)).then(function (r) { return r.json(); })
        ]).then(function (res) {
            var header = (res[0].data || [])[0];
            var details = res[1].data || [];
            deleted = [];

            id("orderDay").value = "";

            id("detailBody").innerHTML = "";
            details.forEach(appendRow);
            if (details.length === 0) {
                appendRow({});
            }

            if (header) {
                id("batchID").value = header.BatchID || "";
                if (header.OrderDay) {
                    var orderDayValue = toDateInputValue(header.OrderDay);
                    if (orderDayValue) {
                        id("orderDay").value = orderDayValue;
                    }
                }
                status("Load thành công.", false);
            } else {
                status("Không tìm thấy BatchID trong dữ liệu DB.", true);
            }

            recalcTotal();
        }).catch(function (err) {
            status("Load thất bại: " + err.message, true);
        });
    }

    function save() {
        var header = getHeaderFromForm();
        if (!header.CpnyID) {
            status("BranchID không được để trống.", true);
            return;
        }

        if (isOrderDayBeforeToday(header.OrderDay)) {
            status("OrderDay không được nhỏ hơn ngày hiện tại (giờ VN).", true);
            return;
        }

        var details = getDetailsFromGrid();
        if (details.length === 0) {
            status("Cần ít nhất 1 dòng detail có InventoryID để lưu.", true);
            return;
        }

        var duplicatedInventory = findDuplicateInventory(details);
        if (duplicatedInventory) {
            status("InventoryID bị trùng trong grid: " + duplicatedInventory + ". Mỗi InventoryID chỉ được xuất hiện 1 lần trong một lô.", true);
            return;
        }

        status("Đang lưu...", false);

        request("/FS10901/Save", {
            header: JSON.stringify(header),
            details: JSON.stringify(details),
            deleted: JSON.stringify(deleted)
        }).then(function (res) {
            if (!res.success) {
                status("Save lỗi: " + (res.message || res.errorMsg || "Unknown error"), true);
                return;
            }

            if (res.batchNbr) {
                ensureBatchOption(res.batchNbr, res.savedOrderDay || header.OrderDay || "");
            }

            if (res.savedOrderDay) {
                id("orderDay").value = res.savedOrderDay;
            }

            deleted = [];
            status("Save thành công.", false);
            loadData();
        }).catch(function (err) {
            status("Save thất bại: " + err.message, true);
        });
    }

    function deleteBatch() {
        var branch = id("branchID").value.trim();
        var batch = id("batchID").value.trim();

        if (!branch || !batch) {
            status("Cần nhập BranchID và BatchID trước khi xóa.", true);
            return;
        }

        if (!window.confirm("Xóa batch " + batch + "?")) {
            return;
        }

        status("Đang xóa...", false);
        request("/FS10901/DeleteData", {
            branchID: branch,
            batchID: batch
        }).then(function (res) {
            if (!res.success) {
                status("Delete lỗi: " + (res.message || res.errorMsg || "Unknown error"), true);
                return;
            }

            id("batchID").value = "";
            deleted = [];
            id("detailBody").innerHTML = "";
            appendRow({});
            status("Delete thành công.", false);
        }).catch(function (err) {
            status("Delete thất bại: " + err.message, true);
        });
    }

    window.fs10901 = {
        addRow: function () { appendRow({}); },
        loadData: loadData,
        save: save,
        deleteBatch: deleteBatch
    };

    window.addEventListener("DOMContentLoaded", function () {
        applyOrderDayMinConstraint();
        setTodayIfEmpty();

        loadComboData();

        var branchEl = id("branchID");
        if (branchEl) {
            branchEl.addEventListener("change", function () {
                populateBatchSelect(branchEl.value);
            });
        }

        var orderDayEl = id("orderDay");
        if (orderDayEl) {
            orderDayEl.addEventListener("change", function () {
                if (isOrderDayBeforeToday(orderDayEl.value)) {
                    orderDayEl.value = getVietnamTodayDateValue();
                    status("Không được chọn ngày trước ngày hiện tại (giờ VN).", true);
                }
            });
        }

        appendRow({});
    });
})();
