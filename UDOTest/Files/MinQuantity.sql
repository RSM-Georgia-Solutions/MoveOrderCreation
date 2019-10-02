 
 SELECT TOP 1 * FROM 
  (
 SELECT SUM("Quantity") as "Quantity", "StorLocCode", max("SSCC") as "SSCC" FROM PMX_INVT WHERE "StorLocCode" 
 in ( 
 SELECT distinct "PMX_INVT"."StorLocCode" FROM "PMX_INVT" 
 INNER JOIN PMX_ITRI on "PMX_INVT"."ItemCode" = "PMX_ITRI"."ItemCode"
 WHERE PMX_INVT."ItemCode" = 'I0387'
 		AND "BestBeforeDate" IN 
 		(
			SELECT distinct "BestBeforeDate" FROM "PMX_INVT" 
 			INNER JOIN PMX_ITRI on "PMX_INVT"."ItemCode" = "PMX_ITRI"."ItemCode"
 			WHERE PMX_INVT."ItemCode" = '$itemCode' AND "BatchNumber" = $BatchNumber
		)
 	 )
    group by "StorLocCode" 
 ) order by "Quantity"