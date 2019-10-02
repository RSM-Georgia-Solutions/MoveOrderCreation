--SET SCHEMA "ULC_TEST";
 
 SELECT    
 RDN1."U_PMX_LUID", -- სადაც დადო Return -მა   
 RDN1."U_PMX_LOCO", -- Storage location code სადაც დადო Return -მა
 RDN1."U_PMX_SLOC", --  Storage location code საიდანაც აიღო (Source)
 RDN1."U_PMX_SSCC", 
 RDN1."LineNum",  
 RDN1."Dscription", 
 RDN1."OpenQty",
 ORDN."DocNum",
 ORDN."DocEntry" AS "Return DocEntry", 
 RDN1."DocEntry",   
 RDN1."ItemCode",  
 RDN1."Quantity",
 RDN1."UomCode", 
 RDN1."NumPerMsr", 
 RDN1."U_PMX_QUAN",  
 RDN1."U_PMX_BATC",  
 PMX_OSWH."Code" AS "PMX WhsCode"
 
  FROM ORDN 
  LEFT OUTER JOIN RDN1 ON ORDN."DocEntry" = RDN1."DocEntry"
  LEFT OUTER JOIN PMX_OSWH ON PMX_OSWH."SboWhsCode" = RDN1."WhsCode"    
  WHERE ORDN.CANCELED = 'N'   AND RDN1."BaseType" != '15'       
  AND  ORDN."DocEntry" not in (select "BaseEntry" from PMX_MOLI where PMX_MOLI."BaseType" = 16)
  AND TO_CHAR(RDN1.U_PMX_LOCO) = 'RB31'	