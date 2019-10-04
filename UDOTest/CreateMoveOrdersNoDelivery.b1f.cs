using System.Collections.Generic;
using SAPbobsCOM;
using SAPbouiCOM.Framework;
using System.Linq;
using MoveOrdersCreation.Models;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace MoveOrdersCreation
{
    [FormAttribute("MoveOrdersCreation.Form2", "CreateMoveOrdersNoDelivery.b1f")]
    sealed class CreateMoveOrdersNoDelivery : UserFormBase
    {
        public CreateMoveOrdersNoDelivery()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Button1 = ((SAPbouiCOM.Button)(this.GetItem("Item_0").Specific));
            this.Button1.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button1_PressedAfter);
            this.Grid0 = ((SAPbouiCOM.Grid)(this.GetItem("Item_1").Specific));
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
        }


        private void OnCustomInitialize()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\QueryNoDelivery.sql");
            string path2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\MinQuantity.sql");
            string path4 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\emptyBin.sql");
            string path3 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\MinQuantityNoItemInBin.sql");
            _query = File.ReadAllText(path);
            _queryMin = File.ReadAllText(path2);
            _queryMinNoItem = File.ReadAllText(path3);
            _queryEmptyBin = File.ReadAllText(path4);
            Refresh();
        }

        private void Refresh()
        {
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte(_query));
        }





        private string _query;
        private string _queryMin;
        private string _queryMinNoItem;
        private string _queryEmptyBin;

        private SAPbouiCOM.Button Button1;

        private void Button1_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Recordset recSetGetReturns = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSet2 = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSet3 = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSet4 = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSet5 = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            List<MoveOrder> moveOrders = new List<MoveOrder>();
            List<MoveOrderRow> moveOrderRows = new List<MoveOrderRow>();

            recSetGetReturns.DoQuery(DiManager.QueryHanaTransalte(_query));

            while (!recSetGetReturns.EoF)
            {
                var itemCode = recSetGetReturns.Fields.Item("ItemCode").Value.ToString();
                var qualityStatus = recSetGetReturns.Fields.Item("U_PMX_QYSC").Value.ToString();
                var batch = recSetGetReturns.Fields.Item("U_PMX_BATC").Value.ToString();
                var docFrom = recSetGetReturns.Fields.Item("U_PMX_LOCO").Value.ToString();
                var srcStorLocCode = docFrom;

                var q1 = _queryMin.Replace("$itemCode", $"{itemCode}");//ეს ეძებს იგივე ვარგისიანობის თარიღით 
                q1 = q1.Replace("$BatchNumber", $"{batch}");
                recSet2.DoQuery(q1);
                int destLogUnitIdentKey;
                int batchId;
                var destiantionLocation = recSet2.Fields.Item("StorLocCode").Value.ToString();

                if (destiantionLocation == string.Empty)
                {
                    recSet3.DoQuery($"{_queryMinNoItem.Replace("$itemCode", $"{itemCode}")}");//ეს ეძებს იგივიე საქონელს ვარგისიანობის ვადის გათვალისწინების გარეშე
                    srcStorLocCode = recSet3.Fields.Item("StorLocCode").Value.ToString();
                    var SSCC = recSet3.Fields.Item("SSCC").Value.ToString();
                    recSet3.DoQuery($"select \"InternalKey\" from PMX_LUID where \"SSCC\" = '{SSCC}'");
                    destLogUnitIdentKey = int.Parse(recSet3.Fields.Item("InternalKey").Value.ToString());
                    recSet5.DoQuery($"select \"InternalKey\" from PMX_ITRI where \"BatchNumber\" = '{batch}'");
                    batchId = int.Parse(recSet5.Fields.Item("InternalKey").Value.ToString());
                    if (string.IsNullOrWhiteSpace(srcStorLocCode))
                    {
                       
                         
                            Application.SBO_Application.MessageBox($"საწყობში ვერ მოიძებნა საქონელი : {itemCode}");
                            recSetGetReturns.MoveNext();
                            continue;
                         

                    }
                }
                else
                {
                    var SSCC = recSet2.Fields.Item("SSCC").Value.ToString();
                    recSet2.DoQuery($"select \"InternalKey\" from PMX_LUID where \"SSCC\" = '{SSCC}'");
                    destLogUnitIdentKey = int.Parse(recSet2.Fields.Item("InternalKey").Value.ToString());
                    recSet2.DoQuery($"select \"InternalKey\" from PMX_ITRI where \"BatchNumber\" = '{batch}'");
                    batchId = int.Parse(recSet2.Fields.Item("InternalKey").Value.ToString());
                }

                var wareHouse = recSetGetReturns.Fields.Item("PMX WhsCode").Value.ToString();
                var lineNum = int.Parse(recSetGetReturns.Fields.Item("LineNum").Value.ToString());
                var baseEntry = int.Parse(recSetGetReturns.Fields.Item("Return DocEntry").Value.ToString());
                var dscription = recSetGetReturns.Fields.Item("Dscription").Value.ToString();
                var quantity = decimal.Parse(recSetGetReturns.Fields.Item("Quantity").Value.ToString());
                var uom = recSetGetReturns.Fields.Item("UomCode").Value.ToString();
                var quantityPerUom = decimal.Parse(recSetGetReturns.Fields.Item("NumPerMsr").Value.ToString());
                var srcLogUnitIdentKey = int.Parse(recSetGetReturns.Fields.Item("U_PMX_LUID").Value.ToString() == string.Empty ? "0" : recSetGetReturns.Fields.Item("U_PMX_LUID").Value.ToString());
                // var destStorLocCode = binTo;
                //   var itemTransactionalInfoKey = int.Parse(recSetGetReturns.Fields.Item("ItemTransactionalInfoKey").Value
                //    .ToString());
                MoveOrder moveOrder = new MoveOrder
                {
                    ToPmxWhsCode = wareHouse,
                    FromPmxWhsCode = wareHouse
                };
                moveOrders.Add(moveOrder);
                MoveOrderRow row = new MoveOrderRow
                {
                    BaseEntry = baseEntry,
                    BaseLine = lineNum,
                    LineNum = lineNum,
                    ItemCode = itemCode,
                    Dscription = dscription,
                    OpenQty = quantity,
                    Quantity = quantity,
                    Uom = uom,
                    QuantityPerUom = quantityPerUom,
                    SrcStorLocCode = srcStorLocCode,
                    SrcLogUnitIdentKey = srcLogUnitIdentKey,
                    DestStorLocCode = destiantionLocation,
                    DestLogUnitIdentKey = destLogUnitIdentKey,
                    ItemTransactionalInfoKey = batchId,
                    StockLevel = 'D',
                    QualityStatusCode = qualityStatus,
                    SrcMasterLogUnitIdentKey = srcLogUnitIdentKey,
                };

                if (!string.IsNullOrEmpty(row.DestStorLocCode))
                {
                    moveOrderRows.Add(row);
                    moveOrder.Rows.Add(row);
                }
                //var result =  moveOrder.Add();

                recSetGetReturns.MoveNext();
            }
            var grouppedMoveOrders = moveOrders.GroupBy(x => new { x.FromPmxWhsCode, x.ToPmxWhsCode });


            List<MoveOrder> moveOrdersPost = new List<MoveOrder>();
            foreach (var moveOrdersGroup in grouppedMoveOrders)
            {

                List<MoveOrderRow> rows = moveOrdersGroup.SelectMany(item => item.Rows).ToList();

                var grupedRowsTmp = rows.GroupBy(x => new { x.DestStorLocCode }).ToList();

                List<List<MoveOrderRow>> rowsxz = new List<List<MoveOrderRow>>();


                foreach (var row in grupedRowsTmp)
                {
                    var groupedRows = new List<MoveOrderRow>();
                    foreach (var item in row)
                    {
                        groupedRows.Add(item);
                    }
                    rowsxz.Add(groupedRows);
                }

                foreach (var item in rowsxz)
                {
                    MoveOrder order = new MoveOrder();

                    order.FromPmxWhsCode = moveOrdersGroup.First().FromPmxWhsCode;
                    order.ToPmxWhsCode = moveOrdersGroup.First().ToPmxWhsCode;

                    order.Rows = new List<MoveOrderRow>();
                    order.Rows = item;
                    moveOrdersPost.Add(order);
                }
            }


            foreach (var item in moveOrdersPost)
            {
                item.Add();
            }
            Refresh();
        }

        private SAPbouiCOM.Grid Grid0;
    }
}