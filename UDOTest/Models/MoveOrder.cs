using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace MoveOrdersCreation.Models
{
    class MoveOrder
    {
        public MoveOrder()
        {
            UserSign = DiManager.Company.UserSignature;
            Canceled = "N";
            CreateDate = DateTime.Now;
            var hours = DateTime.Now.Hour.ToString();
            var minutes = DateTime.Now.Minute.ToString();
            CreateTime = Convert.ToInt32(hours + minutes);
            UpdateDate = DateTime.Now;
            UpdateTime = Convert.ToInt32(hours + minutes);
            Version = 1;
            CreatedBy = DiManager.Company.UserSignature;
            ObjType = "PMX_MOHE";
            Priority = 200;
            DocStatus = 'O';
            MoveOrderType = 'M';
            MoveOrderStatus = 'N';
            DueDate = DateTime.Now;
            LockedBy = null;
            MoveLogUnitIn1Time = 'N';
            Remarks = null;
            Rows = new List<MoveOrderRow>();
        }

        public int UserSign { get; set; }
        public int DocEntry { get; set; }
        public string Canceled { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateTime { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateTime { get; set; }
        public int Version { get; set; } //default value 1
        public int CreatedBy { get; set; } //user id 
        public string ObjType { get; set; } //PMX_MOHE
        public int Priority { get; set; } //200 default value
        public char DocStatus { get; set; } //open = o. closed = c.  ჩვენს შემთხვევაში O
        public char MoveOrderType { get; set; } //M - Normal move order, A - Put away order, R - Replenishment order. ჩვენს შემთხვევაში A
        public char MoveOrderStatus { get; set; } //ჩვენს შემთხვევაში N - Nothing is moved of this move order; C - Move order is closed, P - Move order is partially moved
        public DateTime DueDate { get; set; } //DateTime.Now
        public int? LockedBy { get; set; } //default value = null; The user who locked the move order. Reference to OUSR.INTERNAL_K
        public char MoveLogUnitIn1Time { get; set; }//default value = N; N - Cannot be move in one time, Y - Can be moved in one time, M - Must be moved in one time
        public string FromPmxWhsCode { get; set; } //The source PMX warehouse code. Reference to PMX_OSWH.Code  W02 an W01
        public string ToPmxWhsCode { get; set; } //The destination PMX warehouse code. Reference to PMX_OSWH.Code W02 an W01
        public string Remarks { get; set; } //default value = null უნდა ჩავწეროთ იმ დოკუმენტების ნიმრები რომლებისგანაც შეიქმნა move order

        public List<MoveOrderRow> Rows { get; set; }


        public string Add()
        {
            
            Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            Recordset recSetRow = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);


            string queryHeder = $"INSERT INTO \"PMX_MOHE\" ( \"DocEntry\", \"MoveOrderStatus\", \"DueDate\", \"LockedBy\",  \"Priority\", \"MoveOrderType\", \"MoveLogUnitIn1Time\", \"FromPmxWhsCode\", \"ToPmxWhsCode\",    \"Remarks\", \"DocStatus\", \"Canceled\", \"ObjType\", \"UserSign\", \"CreatedBy\", \"Version\",    \"CreateDate\", \"CreateTime\", \"UpdateDate\", \"UpdateTime\" )   SELECT \"PMX_MOHE_S\".nextval, '{MoveOrderStatus}', '{DueDate.ToString("s")}', '{LockedBy ?? 9999999}', {Priority}, '{MoveOrderType}', '{MoveLogUnitIn1Time}', '{FromPmxWhsCode}', '{ToPmxWhsCode}', '{Remarks}', '{DocStatus}', '{Canceled}', '{ObjType}', '{UserSign}', '{CreatedBy}', '{Version}', '{CreateDate.ToString("s")}', '{CreateTime}', '{UpdateDate.ToString("s")}', '{UpdateTime}' FROM dummy";

            queryHeder = queryHeder.Replace("'9999999'", "null");


            try
            {
                recSet.DoQuery(queryHeder);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            recSet.DoQuery(DiManager.QueryHanaTransalte($"SELECT TOP(1) DocEntry FROM PMX_MOHE ORDER By DocEntry Desc"));
            int docentry = int.Parse(recSet.Fields.Item("DocEntry").Value.ToString());
            DocEntry = docentry;

            int linenum = 0;
            foreach (var row in Rows)
            {
                var src = row.SrcLogUnitIdentKey == 0 ? -99 : row.SrcLogUnitIdentKey;                        
                var src3 = row.DestLogUnitIdentKey == 0 ? -66 : row.DestLogUnitIdentKey;

                string queryRow = $"INSERT INTO \"PMX_MOLI\"(\"InternalKey\", \"MoveOrderLineStatus\", \"SrcLogUnitIdentKey\", \"SrcMasterLogUnitIdentKey\", \"DestLogUnitIdentKey\", \"SrcStorLocCode\", \"QualityStatusCode\", \"DestStorLocCode\", \"OpenQty\", \"OpenQtyUom2\", \"Dscription\", \"ItemTransactionalInfoKey\", \"StockLevel\", \"WABoxCode\", \"DocEntry\", \"LineNum\", \"LineStatus\", \"BaseType\", \"BaseEntry\", \"BaseLine\", \"ItemCode\", \"Quantity\", \"Version\", \"Uom\", \"QuantityPerUom\", \"Uom2\", \"QuantityUom2\") SELECT \"PMX_MOLI_S\".nextval, '{row.MoveOrderLineStatus}', {src},-88,{src3},'{row.SrcStorLocCode}','{row.QualityStatusCode}','{row.DestStorLocCode}',{row.OpenQty}, -33,'{row.Dscription}',{row.ItemTransactionalInfoKey},'{row.StockLevel}',-44,{docentry},{linenum},'{row.LineStatus}','{row.BaseType}',{row.BaseEntry},{linenum},'{row.ItemCode}',{row.Quantity},{row.Version},'{row.Uom}',{row.QuantityPerUom},-111, -55 FROM dummy";
                //QuantityUom2 = null done
                //Uom2 = null  done
                //WABoxCode = null done
                //Uom = სიცარიელე, არც 0 არც null (meore etapi)  not done (არ ყოფილა საჭირო)
                //BaseType, BaseEntry, BaseLine = null (meore etapi)  not done (არ ყოფილა საჭირო)

                queryRow = queryRow.Replace("-99", "NULL");
                queryRow = queryRow.Replace("-88", "NULL");
                queryRow = queryRow.Replace("-77", "NULL");
                queryRow = queryRow.Replace("-66", "NULL");
                queryRow = queryRow.Replace("-111", "NULL");
                queryRow = queryRow.Replace("-111", "NULL");
                queryRow = queryRow.Replace("-44", "NULL");
                queryRow = queryRow.Replace("-55", "NULL");
                queryRow = queryRow.Replace("-33", "NULL");


                try
                {
                    recSet.DoQuery(queryRow);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                linenum++;
            }
            return string.Empty;


        }

        
    }


}
