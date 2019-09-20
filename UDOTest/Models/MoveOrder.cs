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
            MoveOrderType = 'A';
            MoveOrderStatus = 'N';
            DueDate = DateTime.Now;
            LockedBy = null;
            MoveLogUnitIn1Time = 'M';
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

            recSet.DoQuery(DiManager.QueryHanaTransalte($"SELECT TOP(1) DocEntry FROM PMX_MOHE ORDER By DocEntry Desc"));
            int docentry = int.Parse(recSet.Fields.Item("DocEntry").Value.ToString()) + 1;
            DocEntry = docentry;

            string queryHeder = $@"INSERT INTO PMX_MOHE (DocEntry,UserSign, Canceled, CreateDate, CreateTime, UpdateDate, UpdateTime, Version, CreatedBy,  ObjType, Priority, DocStatus, MoveOrderType, MoveOrderStatus, DueDate, LockedBy, MoveLogUnitIn1Time, FromPmxWhsCode, ToPmxWhsCode, Remarks) 
                                            VALUES ({docentry}, {UserSign}, '{Canceled}', '{CreateDate.ToString("s")}', {CreateTime}, '{UpdateDate.ToString("s")}', {UpdateTime}, {Version}, {CreatedBy}, '{ObjType}', {Priority}, '{DocStatus}', '{MoveOrderType}', '{MoveOrderStatus}', '{DueDate.ToString("s")}', '{LockedBy ?? 9999999}', '{MoveLogUnitIn1Time}', '{FromPmxWhsCode}', '{ToPmxWhsCode}', '{Remarks}')";

            queryHeder = queryHeder.Replace("'9999999'", "null");

            try
            {
                recSet.DoQuery(DiManager.QueryHanaTransalte(queryHeder));
            }
            catch (Exception e)
            {
                return e.Message;
            }


            foreach (var row in Rows)
            {
                recSetRow.DoQuery(DiManager.QueryHanaTransalte($"SELECT TOP(1) InternalKey FROM PMX_MOLI ORDER By InternalKey Desc"));
                var internalKey = int.Parse(recSetRow.Fields.Item("InternalKey").Value.ToString()) + 1;
                string queryRow = $@"INSERT INTO PMX_MOLI (InternalKey, DocEntry, LineNum, BaseType, BaseEntry, BaseLine, LineStatus, ItemCode, Dscription, OpenQty, Quantity, Uom, QuantityPerUom, Version, MoveOrderLineStatus, SrcStorLocCode, SrcLogUnitIdentKey, DestStorLocCode, DestLogUnitIdentKey, QualityStatusCode, QuantityUom2, Uom2, OpenQtyUom2, ItemTransactionalInfoKey, StockLevel, WABoxCode, Division, SrcMasterLogUnitIdentKey) 
              VALUES({internalKey}, {docentry}, {row.LineNum}, '{row.BaseType}', {row.BaseEntry}, {row.BaseLine}, '{row.LineStatus}', '{row.ItemCode}', '{row.Dscription}', {row.OpenQty}, 
                      {row.Quantity}, '{row.Uom}', {row.QuantityPerUom}, {row.Version}, '{row.MoveOrderLineStatus}', '{row.SrcStorLocCode}', {row.SrcLogUnitIdentKey}, '{row.DestStorLocCode}', 
                      {row.DestLogUnitIdentKey}, '{row.QualityStatusCode}', {row.QuantityUom2}, '{row.Uom2}', {row.OpenQtyUom2}, {row.ItemTransactionalInfoKey}, '{row.StockLevel}', '{row.WABoxCode}', {row.Division}, {row.SrcMasterLogUnitIdentKey}) ";

                try
                {
                    recSet.DoQuery(DiManager.QueryHanaTransalte(queryRow));
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return string.Empty;


        }


    }


}
