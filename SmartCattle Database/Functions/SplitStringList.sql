USE [smartCattle]
GO
/****** Object:  UserDefinedFunction [dbo].[SplitStringList]    Script Date: 9/23/2018 4:28:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[SplitStringList]
(
	@StrList nvarchar(4000)
)
RETURNS 
@ParsedList table
(
	value nvarchar(500) COLLATE Arabic_CI_AS
)
AS
BEGIN
	DECLARE @Value nvarchar(500), @Pos int

	SET @StrList = LTRIM(RTRIM(@StrList))+ ','
	SET @Pos = CHARINDEX(',', @StrList, 1)

	IF REPLACE(@StrList, ',', '') <> ''
	BEGIN
		WHILE @Pos > 0
		BEGIN
			SET @Value = LTRIM(RTRIM(LEFT(@StrList, @Pos - 1)))
			IF @Value <> ''
			BEGIN
				INSERT INTO @ParsedList (value) 
				VALUES (CAST(@Value AS nvarchar)) --Use Appropriate conversion
			END
			SET @StrList = RIGHT(@StrList, LEN(@StrList) - @Pos)
			SET @Pos = CHARINDEX(',', @StrList, 1)

		END
	END	
	RETURN
END