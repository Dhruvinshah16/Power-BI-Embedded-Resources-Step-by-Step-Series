USE [SamplePBIEmbedded]
GO

/****** Object:  StoredProcedure [dbo].[spInsertProducts]    Script Date: 12/26/2019 9:20:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spInsertProducts]
@ProductType ProductTableType READONLY
AS
BEGIN
    INSERT INTO Product
    SELECT * FROM @ProductType
END
GO


