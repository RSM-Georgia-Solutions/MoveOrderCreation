using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveOrdersCreation.Models
{
    class MoveOrderRow
    {
        public MoveOrderRow()
        {
            BaseType = "16";
            LineStatus = 'O';
            Version = 1;
            MoveOrderLineStatus='N';
            QualityStatusCode = "RELEASED";
            StockLevel = 'D';
        }
        public int InternalKey { get; set; } //Unique key identifying the document 1-n-მდე
        public int DocEntry { get; set; } //The move order this line belongs to. Reference to PMX_MOHE.DocEntry უნდა წამოვიღოთ ეს ველი ჰედერიდან 
        public int LineNum { get; set; } //Number uniquely identifying this line within the document
        public string BaseType { get; set; } //რაზეც არის დაფუძნებული 16-რეთარნზე
        public int BaseEntry { get; set; } //The docentry of the document this line is linking to
        public int BaseLine { get; set; } //The linenum of the document line this line is linking to.
        public char LineStatus { get; set; } //Status of the line: O - Open, C - Closed  
        public string ItemCode { get; set; } //The item code
        public string Dscription { get; set; } //The description of the item involved
        public decimal OpenQty { get; set; } //	The quantity that is still open on the line
        public decimal Quantity { get; set; } //Quantity of items measured in Uom
        public string Uom { get; set; } //Unit-of-measurement for this entry
        public decimal QuantityPerUom { get; set; } //Quantity of items per Uom
        public int Version { get; set; } //Version of the record, increased on update
        public char MoveOrderLineStatus { get; set; } //The status of the move order. N - Nothing is moved of this move order, C - Move order is closed, P - Move order is partially moved
        public string SrcStorLocCode { get; set; } //The source storage location code where the items can be found. Reference to PMX_OSSL.Code
        public int SrcLogUnitIdentKey { get; set; }//The logistic unit where the items can be found now. Reference to PMX_LUID.InternalKey
        public string DestStorLocCode { get; set; } //The destination storage location code where the items must be moved to. Reference to PMX_OSSL.Code
        public int DestLogUnitIdentKey { get; set; } //The logistic unit where the items must be moved to. Reference to PMX_LUID.InternalKey
        public string QualityStatusCode { get; set; } //The quality status code of the item that must be moved. Reference to PMX_QYST.Code
        public decimal QuantityUom2 { get; set; } // default = null Quantity of items measured in Uom2
        public string Uom2 { get; set; } // default = null The secondary unit-of-measurement
        public decimal OpenQtyUom2 { get; set; } // default = null The quantity that is still open on the line for the second UOM.
        public int ItemTransactionalInfoKey { get; set; } //Item transactional information (Batch number, BBD, ...). Reference to PMX_ITRI.InternalKey
        public char StockLevel { get; set; } //The level of stock identification    TO DO (არ ვიცით რას ნიშნავს D I)
        public string WABoxCode { get; set; } // default = null The code of the box. Is used for WA 
        public int Division { get; set; } // default = null The division of the WA box
        public int SrcMasterLogUnitIdentKey { get; set; } // default = null The master logistic unit where the items can be found. Reference to PMX_LUID.InternalKey

    }
}
