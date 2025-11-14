SELECT "Name","ExternalSource","ExternalId"
FROM recipes
WHERE "ExternalSource" IS NOT NULL
ORDER BY "CreatedAt" DESC;

--------------------------------------------------------------------------------

SELECT r."Name", i."Name" AS ingredient, i."Measure"
FROM recipes r
JOIN recipe_ingredients i ON i.recipe_id = r."Id"
ORDER BY r."CreatedAt" DESC;

--------------------------------------------------------------------------------

SELECT COUNT(*) FROM recipes;
SELECT * FROM recipes ORDER BY "CreatedAt" DESC LIMIT 5;