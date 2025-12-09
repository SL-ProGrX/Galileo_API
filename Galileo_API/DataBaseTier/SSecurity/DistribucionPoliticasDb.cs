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

        public List<PaisObtenerDto> PaisObtener()
        {
            List<PaisObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select * from PGX_PAIS";
                    resp = connection.Query<PaisObtenerDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<PaisObtenerDto>();
            }
            return resp;
        }

        public List<ProvinciasObtenerDto> ProvinciasObtener(string CodPais)
        {
            List<ProvinciasObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select Cod_Pais_N1,Descripcion,Activo from PGX_PAIS_N1 where Cod_Pais = @CodPais";
                    resp = connection.Query<ProvinciasObtenerDto>(strSQL, new { CodPais }).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<ProvinciasObtenerDto>();
            }
            return resp;
        }

        public List<CantonesObtenerDto> CantonesObtener(string CodPais, string CodProvincia)
        {
            List<CantonesObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select Cod_Pais_N2,Descripcion,Activo from PGX_PAIS_N2 where Cod_Pais = @CodPais and cod_Pais_N1 = @CodProvincia";
                    
                    
                    resp = connection.Query<CantonesObtenerDto>(strSQL, new { CodPais, CodProvincia }).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<CantonesObtenerDto>();
            }
            return resp;
        }

        public List<DistritosObtenerDto> DistritosObtener(string CodPais, string CodProvincia, string CodCanton)
        {
            List<DistritosObtenerDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = "select Cod_Pais_N2,Descripcion,Activo from PGX_PAIS_N3 where Cod_Pais = @CodPais and cod_Pais_N1 = @CodProvincia and cod_Pais_N2 = @CodCanton";
                    
                    
                    
                    resp = connection.Query<DistritosObtenerDto>(strSQL, new { CodPais, CodProvincia, CodCanton }).ToList();
                }
            }
            catch (Exception)
            {
                resp = new List<DistritosObtenerDto>();
            }
            return resp;
        }

        public ErrorDto FxGuardar(GuardarDto dto)
        {
            ErrorDto result = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    string strSQL;
                    int existe;

                    switch (dto.VModifica.ToUpper())
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
            catch (Exception)
            {
                // Log the error
                result.Code = 0;
                result.Description = "Ok";
                return result;
            }
        }
    }
}
