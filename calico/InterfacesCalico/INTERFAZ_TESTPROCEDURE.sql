USE [DB_INTERFAZ]
GO
/****** Object:  StoredProcedure [dbo].[INTERFAZ_TESTPROCEDURE]    Script Date: 28/4/2019 12:56:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[INTERFAZ_TESTPROCEDURE]
	-- Add the parameters for the stored procedure here
	@Nombre varchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT CAST(GETDATE() AS DATE) -- el parametro se usa campo = @Nombre
END
