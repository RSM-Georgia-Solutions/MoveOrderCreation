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
            _query = File.ReadAllText(path);
            _queryMin = File.ReadAllText(path2);
            Refresh();
        }

        private void Refresh()
        {
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte(_query));
        }





        private  string _query ;
        private  string _queryMin;
        private SAPbouiCOM.Button Button1;

        private void Button1_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Recordset recSetGetReturns = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);   
            Recordset recSet2 = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            List<MoveOrder> moveOrders = new List<MoveOrder>();
            List<MoveOrderRow> moveOrderRows = new List<MoveOrderRow>();

            recSetGetReturns.DoQuery(DiManager.QueryHanaTransalte(_query));

            while (!recSetGetReturns.EoF)
            {
                var itemCode = recSetGetReturns.Fields.Item("ItemCode").Value.ToString();
                var batch = recSetGetReturns.Fields.Item("U_PMX_BATC").Value.ToString();
                _queryMin = _queryMin.Replace("$itemCode", $"{itemCode}");
                _queryMin = _queryMin.Replace("$BatchNumber", $"{batch}");
                recSet2.DoQuery(_queryMin);
                var destiantionLocation = recSet2.Fields.Item("StorLocCode").Value.ToString();
                if (destiantionLocation == string.Empty)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.MessageBox($"Item : {itemCode} Not Found in Warehouse");
                    recSetGetReturns.MoveNext();
                    continue;
                }
                var SSCC = recSet2.Fields.Item("SSCC").Value.ToString();
                recSet2.DoQuery($"select \"InternalKey\" from PMX_LUID where \"SSCC\" = '{SSCC}'");
                var destLogUnitIdentKey = int.Parse(recSet2.Fields.Item("InternalKey").Value.ToString());
                recSet2.DoQuery($"select \"InternalKey\" from PMX_ITRI where \"BatchNumber\" = '{batch}'");
                var batchId = int.Parse(recSet2.Fields.Item("InternalKey").Value.ToString());
                //var binTo = recSetGetReturns.Fields.Item("StorLocCode").Value.ToString();                               
                // int destLogUnitIdentKey = int.Parse(recSetGetReturns.Fields.Item("LogUnitIdentKey").Value.ToString());
                var docFrom = recSetGetReturns.Fields.Item("U_PMX_LOCO").Value.ToString();
                var wareHouse = recSetGetReturns.Fields.Item("PMX WhsCode").Value.ToString();
                var lineNum = int.Parse(recSetGetReturns.Fields.Item("LineNum").Value.ToString());
                var baseEntry = int.Parse(recSetGetReturns.Fields.Item("Return DocEntry").Value.ToString());
                var dscription = recSetGetReturns.Fields.Item("Dscription").Value.ToString();                 
                var quantity = decimal.Parse(recSetGetReturns.Fields.Item("Quantity").Value.ToString());
                var uom = recSetGetReturns.Fields.Item("UomCode").Value.ToString();
                var quantityPerUom = decimal.Parse(recSetGetReturns.Fields.Item("NumPerMsr").Value.ToString());
                var srcStorLocCode = docFrom;
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
                    SrcMasterLogUnitIdentKey = srcLogUnitIdentKey,
                };


                moveOrderRows.Add(row);
                moveOrder.Rows.Add(row);
                //var result =  moveOrder.Add();

                recSetGetReturns.MoveNext();
            }
            var grouppedMoveOrders = moveOrders.GroupBy(x => new { x.FromPmxWhsCode, x.ToPmxWhsCode });


            List<MoveOrder> moveOrdersPost = new List<MoveOrder>();
            foreach (var moveOrdersGroup in grouppedMoveOrders)
            {
                MoveOrder order = moveOrdersGroup.First();

                List<MoveOrderRow> rows = moveOrdersGroup.SelectMany(item => item.Rows).ToList();


                order.Rows = new List<MoveOrderRow>();
                order.Rows = rows;

                moveOrdersPost.Add(order);
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