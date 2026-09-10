using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class DistribucionPoliticasDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";


        public DistribucionPoliticasDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<PaisObtenerDto>> PaisObtener()
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    const string strSQL = "SELECT COD_PAIS AS CodPais, DESCRIPCION AS Descripcion, ZONA_HORARIA AS ZonaHoraria, ACTIVO AS Activo, N1_NOMBRE AS N1Nombre, N2_NOMBRE AS N2Nombre, N3_NOMBRE AS N3Nombre, REGISTRO_FECHA AS RegistroFecha, REGISTRO_USUARIO AS RegistroUsuario FROM [PGX_Portal].[dbo].[PGX_PAIS] WHERE ACTIVO = 1 ORDER BY COD_PAIS";
                    return DbHelper.CreateOkResponse(connection.Query<PaisObtenerDto>(strSQL).ToList());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<PaisObtenerDto>>(ex.Message);
            }
        }

        public ErrorDto<List<ProvinciasObtenerDto>> ProvinciasObtener(string CodPais)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    const string strSQL = "SELECT COD_PAIS_N1 AS CodPaisN1, DESCRIPCION AS Descripcion, ACTIVO AS Activo FROM [PGX_Portal].[dbo].[PGX_PAIS_N1] WHERE ACTIVO = 1 AND COD_PAIS = @CodPais ORDER BY COD_PAIS_N1";
                    return DbHelper.CreateOkResponse(connection.Query<ProvinciasObtenerDto>(strSQL, new { CodPais }).ToList());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<ProvinciasObtenerDto>>(ex.Message);
            }
        }

        public ErrorDto<List<CantonesObtenerDto>> CantonesObtener(string CodPais, string CodProvincia)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    const string strSQL = "SELECT COD_PAIS_N2 AS CodPaisN2, DESCRIPCION AS Descripcion, ACTIVO AS Activo FROM [PGX_Portal].[dbo].[PGX_PAIS_N2] WHERE ACTIVO = 1 AND COD_PAIS = @CodPais AND COD_PAIS_N1 = @CodProvincia ORDER BY COD_PAIS_N2";
                    return DbHelper.CreateOkResponse(connection.Query<CantonesObtenerDto>(strSQL, new { CodPais, CodProvincia }).ToList());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CantonesObtenerDto>>(ex.Message);
            }
        }

        public ErrorDto<List<DistritosObtenerDto>> DistritosObtener(string CodPais, string CodProvincia, string CodCanton)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    const string strSQL = "SELECT COD_PAIS_N3 AS CodPaisN3, DESCRIPCION AS Descripcion, ACTIVO AS Activo FROM [PGX_Portal].[dbo].[PGX_PAIS_N3] WHERE ACTIVO = 1 AND COD_PAIS = @CodPais AND COD_PAIS_N1 = @CodProvincia AND COD_PAIS_N2 = @CodCanton ORDER BY COD_PAIS_N3";
                    return DbHelper.CreateOkResponse(connection.Query<DistritosObtenerDto>(strSQL, new { CodPais, CodProvincia, CodCanton }).ToList());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DistritosObtenerDto>>(ex.Message);
            }
        }

        public ErrorDto FxGuardar(GuardarDto dto)
        {
            if (dto is null)
            {
                return DbHelper.ErrorResponse("La información a guardar es requerida.");
            }

            var tipoOperacion = dto.VModifica.Trim().ToUpperInvariant();
            if (tipoOperacion is not ("P" or "C" or "D"))
            {
                return DbHelper.ErrorResponse("El nivel de distribución política no es válido.");
            }

            var result = DbHelper.CreateOkResponse();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    string strSQL;
                    int existe;

                    switch (tipoOperacion)
                    {
                        case "P":
                            strSQL = "SELECT COUNT(*) FROM Provincias WHERE Provincia = @Provincia";
                            existe = connection.ExecuteScalar<int>(strSQL, new { Provincia = dto.Provincia });

                            if (existe == 0)
                            {
                                strSQL = "INSERT INTO provincias(provincia, descripcion) VALUES(@Provincia, @Descripcion)";
                                connection.Execute(strSQL, new { Provincia = dto.Provincia, Descripcion = dto.Descripcion });
                            }
                            else
                            {
                                strSQL = "UPDATE provincias SET descripcion = @Descripcion WHERE Provincia = @Provincia";
                                connection.Execute(strSQL, new { Provincia = dto.Provincia, Descripcion = dto.Descripcion });
                                ////Bitacora("Modifica", "Provincia : " + dto.Provincia);
                            }
                            result.Code = 1;
                            result.Description = "Ok";
                            break;

                        case "C":
                            strSQL = "SELECT COUNT(*) FROM Cantones WHERE Provincia = @TagProvincia AND Canton = @Canton";
                            existe = connection.ExecuteScalar<int>(strSQL, new { TagProvincia = dto.TagProvincia, Canton = dto.Canton });

                            if (existe == 0)
                            {
                                strSQL = "INSERT INTO cantones(provincia, canton, descripcion) VALUES(@TagProvincia, @Canton, @Descripcion)";
                                connection.Execute(strSQL, new { TagProvincia = dto.TagProvincia, Canton = dto.Canton, Descripcion = dto.Descripcion });
                            }
                            else
                            {
                                strSQL = "UPDATE cantones SET descripcion = @Descripcion WHERE Provincia = @TagProvincia AND Canton = @Canton";
                                connection.Execute(strSQL, new { TagProvincia = dto.TagProvincia, Canton = dto.Canton, Descripcion = dto.Descripcion });
                            }
                            result.Code = 1;
                            result.Description = "Ok";
                            break;

                        case "D":
                            strSQL = "SELECT COUNT(*) FROM Distritos WHERE Provincia = @TagProvincia AND Canton = @TagCanton AND Distrito = @Distrito";
                            existe = connection.ExecuteScalar<int>(strSQL, new { TagProvincia = dto.TagProvincia, TagCanton = dto.TagCanton, Distrito = dto.Distrito });

                            if (existe == 0)
                            {
                                strSQL = "INSERT INTO distritos(provincia, canton, distrito, descripcion) VALUES(@TagProvincia, @TagCanton, @Distrito, @Descripcion)";
                                connection.Execute(strSQL, new { TagProvincia = dto.TagProvincia, TagCanton = dto.TagCanton, Distrito = dto.Distrito, Descripcion = dto.Descripcion });
                            }
                            else
                            {
                                strSQL = "UPDATE distritos SET descripcion = @Descripcion WHERE Provincia = @TagProvincia AND Canton = @TagCanton AND Distrito = @Distrito";
                                connection.Execute(strSQL, new { TagProvincia = dto.TagProvincia, TagCanton = dto.TagCanton, Distrito = dto.Distrito, Descripcion = dto.Descripcion });
                            }
                            result.Code = 1;
                            result.Description = "Ok";
                            break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
