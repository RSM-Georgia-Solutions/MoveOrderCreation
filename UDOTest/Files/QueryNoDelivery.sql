SELECT  
*
  FROM ORDN 
  LEFT OUTER JOIN RDN1 ON ORDN."DocEntry" = RDN1."DocEntry"
  LEFT OUTER JOIN PMX_OSWH ON PMX_OSWH."SboWhsCode" = RDN1."WhsCode"    
  WHERE ORDN.CANCELED = 'N'   AND RDN1."BaseType" != '15'       
  AND  ORDN."DocEntry" not in (select "BaseEntry" from PMX_MOLI where PMX_MOLI."BaseType" = 16)
  AND TO_CHAR(RDN1.U_PMX_LOCO) = 'R03'	