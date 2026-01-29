using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;

namespace Galileo_API.DataBaseTier.TES
{
    public class FrmTesTokenDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria _mtes;

        public FrmTesTokenDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mtes = new MTesoreria(config);
        }

        /// <summary>
        /// Obtiene los primeros 100 tokens de la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tokens.</returns>
        public ErrorDto<List<TesTokenDto>> TES_Token_Top_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                                    SELECT TOP 100 
                                        Tok.ID_TOKEN AS IdToken,
                                        Tok.ESTADO AS Estado,
                                        Tok.REGISTRO_FECHA AS RegistroFecha,
                                        Tok.REGISTRO_USUARIO AS RegistroUsuario,
                                        ISNULL(COUNT(*), 0) AS Pendiente,
                                        ISNULL(SUM(Tra.Monto), 0) AS Monto
                                    FROM TES_TOKENS Tok
                                    LEFT JOIN TES_TRANSACCIONES Tra ON Tok.ID_TOKEN = Tra.ID_TOKEN AND Tra.ESTADO = 'P'
                                    GROUP BY Tok.ID_TOKEN, Tok.ESTADO, Tok.REGISTRO_FECHA, Tok.REGISTRO_USUARIO
                                    ORDER BY Tok.REGISTRO_FECHA DESC";

                return conn.Query<TesTokenDto>(query).ToList();
            });
        }


        /// <summary>
        /// Cierra un token en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Id">ID del token a cerrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TES_Token_Cerrar(int CodEmpresa, string Id)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                // Verificar si el token está activo
                string querySelect = "SELECT id_token FROM tes_tokens WHERE estado = 'A' AND id_token = @Id";
                var tokenActivo = conn.QueryFirstOrDefault<string>(querySelect, new { Id });

                if (tokenActivo == null)
                {
                    return DbHelper.ErrorResponse("Este token ya está cerrado.", -2);
                }

                // Cerrar el token
                string queryUpdate = "UPDATE tes_tokens SET estado = 'C' WHERE id_token = @Id";
                conn.Execute(queryUpdate, new { Id });

                return DbHelper.OkResponse("Token cerrado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    
        public ErrorDto<List<TesTokenSolicitudesData>> TES_Token_Pen_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<TesTokenSolicitudesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesTokenSolicitudesData>()
            };
            try
            {
                // Verificar si el token está activo
                var procedure = $@"[spPres_TokenPendientes_Obtener]";

                response.Result = conn.Query<TesTokenSolicitudesData>(procedure, commandType: System.Data.CommandType.StoredProcedure).ToList();

            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<List<TesTokenSolicitudesData>>(ex.Message);
            }

            return response;
        }


        public ErrorDto TES_Token_Pen_Incluir(int CodEmpresa, string token ,List<string> solicitudes)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var errores = new System.Text.StringBuilder();
                foreach (var solicitud in solicitudes)
                {

                    var procedure = $@"[spPres_TokenPendientes_Actualizar]";
                    var values = new
                    {
                        token = token,
                        nSolicitid = solicitud
                    };
                        try
                        {
                            conn.Query<object>(procedure, values, commandType: System.Data.CommandType.StoredProcedure);
                        }
                        catch (Exception e)
                        {
                            errores.AppendLine(solicitud + ": " + e.Message);
                        }
                }

                if (errores.Length > 0)
                {
                    return DbHelper.ErrorResponse(errores.ToString());
                }

                return DbHelper.OkResponse("Operación realizada satisfactoriamente.");
            }
            catch (Exception ex)
            {
               return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto TES_Token_Crear(int CodEmpresa, string Usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, Usuario);
        }
    }
}