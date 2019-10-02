SELECT  
 PMX_PLPL."DocEntry" as "Pick Proposal List DocEntry",
 PMX_PLHE."DocEntry" as "Pick List DocEntry",
 RDN1."U_PMX_LUID", -- სადაც დადო Return -მა 
 PMX_INVT."LogUnitIdentKey", -- LUID სადაც დადო Return -მა 
 PMX_PLLI."LogUnitIdentKey", -- LUID საიდანაც აიღო Pick List -მა
 RDN1."U_PMX_LOCO", -- Storage location code სადაც დადო Return -მა
 RDN1."U_PMX_SLOC", --  Storage location code საიდანაც აიღო (Source)
 PMX_PLHE."DestStorLocCode", -- საიდაც დადო Pick List -მა
 PMX_PLPL."ItemTransactionalInfoKey", 
 DLN1."CodeBars",
 RDN1."U_PMX_SSCC", 
 RDN1."LineNum",  
 RDN1."Dscription", 
 RDN1."OpenQty",
 ORDN."DocNum",
 ORDN."DocEntry" AS "Return DocEntry", 
 RDN1."DocEntry", 
 ODLN."DocEntry" AS "Delivery DocEntry",
 ODLN."DocNum" AS "Delivery DocNum", 
 RDN1."ItemCode",
 PMX_PLLI."StorLocCode", 
 RDN1."Quantity",
 RDN1."UomCode", 
 RDN1."NumPerMsr", 
 RDN1."U_PMX_QUAN",  
 PMX_OSWH."Code" AS "PMX WhsCode",
 DLN1."unitMsr",
 DLN1."NumPerMsr" 
              FROM ORDN 
              LEFT OUTER JOIN RDN1 ON ORDN."DocEntry" = RDN1."DocEntry" 
              LEFT OUTER JOIN DLN1 ON RDN1."BaseEntry" = DLN1."DocEntry" AND RDN1."LineNum" = DLN1."LineNum" 
              LEFT OUTER JOIN ODLN ON ODLN."DocEntry" = DLN1."DocEntry" 
              LEFT OUTER JOIN PMX_PLPL ON DLN1."BaseEntry" = PMX_PLPL."BaseEntry" AND DLN1."LineNum" = PMX_PLPL."LineNum" 
              LEFT OUTER JOIN PMX_PLLI ON PMX_PLPL."DocEntry" = PMX_PLLI."BaseEntry" AND PMX_PLPL."LineNum" = PMX_PLLI."LineNum" 
              LEFT OUTER JOIN PMX_PLHE ON PMX_PLLI."DocEntry" = PMX_PLHE."DocEntry"        
              LEFT OUTER JOIN PMX_OSWH ON PMX_OSWH."SboWhsCode" = RDN1."WhsCode"   
              LEFT OUTER JOIN PMX_INVT ON PMX_INVT."ItemCode" = RDN1."ItemCode" AND PMX_INVT."StorLocCode" = PMX_PLLI."StorLocCode"              
              WHERE ORDN.CANCELED = 'N' AND ODLN.CANCELED = 'N' AND RDN1."BaseType" = '15' AND DLN1."BaseType" = '17' AND PMX_PLPL."BaseType" = '17'     
              AND  ORDN."DocEntry" not in (select "BaseEntry" from PMX_MOLI where PMX_MOLI."BaseType" = 16)        
  			  AND TO_CHAR(RDN1.U_PMX_LOCO) = 'R03'	
  				  
  				