 
 SELECT TOP 1 * FROM 
  (
 SELECT SUM("Quantity") as "Quantity", "StorLocCode", max("SSCC") as "SSCC" FROM PMX_INVT WHERE "StorLocCode" 
 in ( 
 SELECT distinct "PMX_INVT"."StorLocCode" FROM "PMX_INVT" 
 INNER JOIN PMX_ITRI on "PMX_INVT"."ItemCode" = "PMX_ITRI"."ItemCode"
 WHERE PMX_INVT."ItemCode" = '$itemCode'
 		AND "BestBeforeDate" IN 
 		(
			SELECT distinct "BestBeforeDate" FROM "PMX_INVT" 
 			INNER JOIN PMX_ITRI on "PMX_INVT"."ItemCode" = "PMX_ITRI"."ItemCode"
 			WHERE PMX_INVT."ItemCode" = '$itemCode' AND "BatchNumber" = '$BatchNumber'
		) AND "StorLocCode" in (select "Code" from PMX_OSBI)-- დოკები არ გვჭირდება 
		--AND "StorLocCode" in (SELECT "Code" from PMX_OSSL WHERE "IsPickLoc" = 'Y')--ისეთი ბინები სადაც აგროვება ხდება
 	 )
    group by "StorLocCode" 
 ) order by "Quantity"