using System.Collections.Generic;
using SAPbobsCOM;
using SAPbouiCOM.Framework;
using System.Linq;
using MoveOrdersCreation.Models;

namespace MoveOrdersCreation
{
    [FormAttribute("MoveOrdersCreation.Form1", "CreateMoveOrders.b1f")]
    sealed class CreateMoveOrders : UserFormBase
    {
        public CreateMoveOrders()
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
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte(_query));
        }

        private readonly string _query = $@"SELECT    
   DLN1.CodeBars, 
   RDN1.LineNum,
   PMX_PLPL.ItemTransactionalInfoKey,
   PMX_MOLI.BaseType, 
   RDN1.Dscription,
   PMX_PLPL.LogUnitIdentKey,
   RDN1.OpenQty,
   ORDN.DocNum,
   ORDN.DocEntry as 'Return DocEntry',
   RDN1.DocEntry,
   ODLN.DocEntry as 'Delivery DocEntry',
   ODLN.DocNum as 'Delivery DocNum',
   PMX_PLHE.DestStorLocCode,   
   RDN1.ItemCode, 
   PMX_PLLI.StorLocCode, 
   RDN1.Quantity,      
   RDN1.UomCode,      
   RDN1.NumPerMsr,
   RDN1.U_PMX_QUAN,      
   RDN1.U_PMX_LOCO, 
   RDN1.U_PMX_LUID,
   RDN1.U_PMX_SSCC,
   PMX_OSWH.Code as 'PMX WhsCode',
   DLN1.unitMsr,
   DLN1.NumPerMsr
 from ORDN
  left join RDN1 on ORDN.DocEntry = RDN1.DocEntry  
  left join DLN1 on RDN1.BaseEntry = DLN1.DocEntry AND RDN1.LineNum = DLN1.LineNum
  left join ODLN on ODLN.DocEntry = DLN1.DocEntry
  left join PMX_PLPL on DLN1.BaseEntry = PMX_PLPL.BaseEntry AND DLN1.LineNum = PMX_PLPL.LineNum
  left join PMX_PLLI on PMX_PLPL.DocEntry = PMX_PLLI.BaseEntry AND PMX_PLPL.LineNum = PMX_PLLI.LineNum
  left Join PMX_PLHE on PMX_PLLI.DocEntry = PMX_PLHE.DocEntry
  left Join PMX_MOLI on PMX_MOLI.BaseEntry = ORDN.DocEntry
  left join PMX_OSWH on PMX_OSWH.SboWhsCode = RDN1.WhsCode
  
  WHERE  ORDN.CANCELED = 'N' AND ODLN.CANCELED = 'N' AND RDN1.BaseType = '15' AND DLN1.BaseType = '17'
  AND PMX_PLPL.BaseType = '17'
  AND (PMX_MOLI.BaseType != '16' OR PMX_MOLI.BaseType is null)
  AND TO_CHAR(RDN1.U_PMX_LOCO) = 'R03'";






        private SAPbouiCOM.Button Button1;

        private void Button1_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Recordset recSetGetReturns = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSetGetMoveOrderRows = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSetGetMoveOrderRowsLuId = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);

            //ItemCode
            //BachId
            //SSCC
            //PICKED FROM

            List<MoveOrder> moveOrders = new List<MoveOrder>();
            List<MoveOrderRow> moveOrderRows = new List<MoveOrderRow>();


            recSetGetReturns.DoQuery(DiManager.QueryHanaTransalte(_query));

            while (!recSetGetReturns.EoF)
            {
                recSetGetMoveOrderRows.DoQuery(DiManager.QueryHanaTransalte($@"SELECT * FROM PMX_MOLI"));
                var itemCode = recSetGetReturns.Fields.Item("ItemCode").Value.ToString();
                var binTo = recSetGetReturns.Fields.Item("StorLocCode").Value.ToString();
                var plplLuId = recSetGetReturns.Fields.Item("LogUnitIdentKey").Value.ToString();
                recSetGetMoveOrderRowsLuId.DoQuery(DiManager.QueryHanaTransalte($"select * from PMX_INVD where StorLocCode = '{binTo}' AND LogUnitIdentKey = '{plplLuId}'"));
                int destLogUnitIdentKey = recSetGetMoveOrderRowsLuId.EoF ? int.Parse(recSetGetReturns.Fields.Item("U_PMX_LUID").Value.ToString() == string.Empty ? "0" : recSetGetReturns.Fields.Item("U_PMX_LUID").Value.ToString()) : int.Parse(plplLuId);
                var docFrom = recSetGetReturns.Fields.Item("U_PMX_LOCO").Value.ToString();
                var wareHouse = recSetGetReturns.Fields.Item("PMX WhsCode").Value.ToString();
                var lineNum = int.Parse(recSetGetReturns.Fields.Item("LineNum").Value.ToString());
                var baseEntry = int.Parse(recSetGetReturns.Fields.Item("Return DocEntry").Value.ToString());
                var dscription = recSetGetReturns.Fields.Item("Dscription").Value.ToString();
                var openQty = decimal.Parse(recSetGetReturns.Fields.Item("OpenQty").Value.ToString());
                var quantity = decimal.Parse(recSetGetReturns.Fields.Item("Quantity").Value.ToString());
                var uom = recSetGetReturns.Fields.Item("UomCode").Value.ToString();
                var quantityPerUom = decimal.Parse(recSetGetReturns.Fields.Item("NumPerMsr").Value.ToString());
                var srcStorLocCode = docFrom;
                var srcLogUnitIdentKey = int.Parse(recSetGetReturns.Fields.Item("U_PMX_LUID").Value.ToString() == string.Empty ? "0" : recSetGetReturns.Fields.Item("U_PMX_LUID").Value.ToString());
                var destStorLocCode = binTo;
                var itemTransactionalInfoKey = int.Parse(recSetGetReturns.Fields.Item("ItemTransactionalInfoKey").Value
                    .ToString());
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
                    DestStorLocCode = destStorLocCode,
                    DestLogUnitIdentKey = destLogUnitIdentKey,
                    ItemTransactionalInfoKey = itemTransactionalInfoKey,
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

        }

        private SAPbouiCOM.Grid Grid0;
    }
}