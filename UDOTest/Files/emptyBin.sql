 SELECT TOP 1 * FROM 
  (
 SELECT SUM("Quantity") as "Quantity", "StorLocCode", max("SSCC") as "SSCC" FROM PMX_INVT WHERE "StorLocCode" 
 in ( 
 SELECT distinct "PMX_INVT"."StorLocCode" FROM "PMX_INVT" 
 INNER JOIN PMX_ITRI on "PMX_INVT"."ItemCode" = "PMX_ITRI"."ItemCode"    
 	 )
    group by "StorLocCode" 
 ) order by "Quantity"