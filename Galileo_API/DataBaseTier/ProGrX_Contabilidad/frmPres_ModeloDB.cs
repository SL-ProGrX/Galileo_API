using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresModeloDb
    {
        private readonly IConfiguration _config;

        public FrmPresModeloDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(stringConn);
        }

        #endregion

        /// <summary>
        /// Obtener las contabilidades por empresa
        /// </summary>
        public ErrorDto<List<CntxCData>> CntxContabilidades_Obtener(int codEmpresa)
        {
            var resp = new ErrorDto<List<CntxCData>>
            {
                Code = 0,
                Result = new List<CntxCData>()
            };

            const string sql = @"
                SELECT cod_contabilidad AS IdX,
                       Nombre          AS ItmX
                FROM CNTX_Contabilidades
                ORDER BY cod_Contabilidad;";

            try
            {
                using var connection = CreateConnection(codEmpresa);
                resp.Result = connection.Query<CntxCData>(sql).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "CntxContabilidades_Obtener: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene cierres
        /// </summary>
        public ErrorDto<List<CntxCData>> CntxCierres_Obtener(int codEmpresa, int codContab)
        {
            var resp = new ErrorDto<List<CntxCData>>
            {
                Code = 0,
                Result = new List<CntxCData>()
            };

            const string sql = @"
                SELECT ID_CIERRE AS IdX,
                       DESCRIPCION AS ItmX
                FROM CNTX_CIERRES
                WHERE COD_CONTABILIDAD = @CodContab
                ORDER BY INICIO_ANIO DESC;";

            try
            {
                using var connection = CreateConnection(codEmpresa);
                resp.Result = connection.Query<CntxCData>(sql, new { CodContab = codContab }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "CntxCierres_Obtener: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtener Modelo
        /// </summary>
        public ErrorDto<PresModeloData> Pres_Modelo_Obtener(int codEmpresa, string codModelo, int codContab)
        {
            var resp = new ErrorDto<PresModeloData>
            {
                Code = 0,
                Result = new PresModeloData()
            };

            const string proc = "[spPres_ModelosConsulta]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodModelo = codModelo,
                    CodContab = codContab
                };

                resp.Result = connection.QueryFirstOrDefault<PresModeloData>(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Hacer scroll en los modelos
        /// </summary>
        public ErrorDto<PresModeloData> Pres_Modelo_scroll(int codEmpresa, int scrollValue, string? codModelo, int codContab)
        {
            var resp = new ErrorDto<PresModeloData>
            {
                Code = 0,
                Result = new PresModeloData()
            };

            const string sqlNext = @"
                SELECT TOP 1 COD_MODELO
                FROM PRES_MODELOS
                WHERE cod_contabilidad = @CodContab
                  AND COD_MODELO > @CodModelo
                ORDER BY COD_MODELO ASC;";

            const string sqlPrev = @"
                SELECT TOP 1 COD_MODELO
                FROM PRES_MODELOS
                WHERE cod_contabilidad = @CodContab
                  AND COD_MODELO < @CodModelo
                ORDER BY COD_MODELO DESC;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo
                };

                var sql = scrollValue == 1 ? sqlNext : sqlPrev;

                resp.Result = connection.QueryFirstOrDefault<PresModeloData>(sql, parameters);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_scroll: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Lista de Modelos</summary>
        public ErrorDto<List<PresModeloData>> Pres_Modelos_Lista(int codEmpresa, int codContab)
        {
            var resp = new ErrorDto<List<PresModeloData>>
            {
                Code = 0,
                Result = new List<PresModeloData>()
            };

            const string sql = @"
                SELECT COD_MODELO,
                       Descripcion 
                FROM PRES_MODELOS 
                WHERE COD_CONTABILIDAD = @CodContab 
                ORDER BY COD_MODELO;";

            try
            {
                using var connection = CreateConnection(codEmpresa);
                resp.Result = connection.Query<PresModeloData>(sql, new { CodContab = codContab }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelos_Lista: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>Insertar Modelo</summary>
        public ErrorDto Pres_Modelo_Insertar(int codEmpresa, PresModeloInsert request)
        {
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            const string proc = "[spPres_ModelosRegistra]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var estado = Strings.Mid(request.Estado, 1, 1); // Mantengo tu lógica VB

                var parameters = new
                {
                    CodModelo = request.Cod_Modelo,
                    CodContab = request.Cod_Contabilidad,
                    IdCierre = request.ID_Cierre,
                    Descripcion = request.Descripcion,
                    Notas = request.Notas,
                    Estado = estado,
                    Usuario = request.Usuario
                };

                connection.Execute(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Información guardada satisfactoriamente...";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Insertar: " + ex.Message;
            }
            return resp;
        }

        /// <summary>Mapea Cuentas sin Centro Costo</summary>
        public ErrorDto Pres_MapeaCuentasSinCentroCosto_SP(int codEmpresa, string codModelo, int codContab, string usuario)
        {
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            const string proc = "[spPres_MapeaCuentasSinCentroCosto]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodModelo = codModelo,
                    CodContab = codContab,
                    Usuario = usuario
                };

                connection.Execute(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Revisión de Mapeo de Cuentas sin Centro de Costo, realizado satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_MapeaCuentasSinCentroCosto_SP: " + ex.Message;
            }
            return resp;
        }

        /// <summary>Reiniciar Modelo</summary>
        public ErrorDto Pres_Model_Reiniciar(int codEmpresa, string codModelo)
        {
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            const string sqlDeletePresupuesto = @"
                DELETE FROM PRES_PRESUPUESTO
                WHERE COD_MODELO = @CodModelo;";

            const string sqlDeleteAjustes = @"
                DELETE FROM PRES_PRESUPUESTO_AJUSTES
                WHERE COD_MODELO = @CodModelo;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new { CodModelo = codModelo };

                connection.Execute(sqlDeletePresupuesto, parameters);
                connection.Execute(sqlDeleteAjustes, parameters);

                resp.Description = "Modelo de Presupuesto inicializado, vuelva a cargar las cuentas!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Model_Reiniciar: " + ex.Message;
            }
            return resp;
        }

        /// <summary>Obtiene los usuarios y ajustes de un modelo</summary>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_SP(int codEmpresa, string codModelo, int codContab)
        {
            var resp = new ErrorDto<List<PressModeloUsuarios>>
            {
                Code = 0,
                Result = new List<PressModeloUsuarios>()
            };

            const string proc = "[spPres_Modelo_Usuarios]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo
                };

                resp.Result = connection
                    .Query<PressModeloUsuarios>(
                        proc,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Usuarios_SP: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Obtiene los ajustes de un modelo</summary>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_SP(int codEmpresa, string codModelo, int codContab)
        {
            var resp = new ErrorDto<List<PressModeloAjustes>>
            {
                Code = 0,
                Result = new List<PressModeloAjustes>()
            };

            const string proc = "[spPres_Modelo_Ajustes]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo
                };

                resp.Result = connection
                    .Query<PressModeloAjustes>(
                        proc,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Ajustes_SP: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Obtiene los ajustes y usuarios autorizados de un modelo</summary>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_Autorizados_SP(int codEmpresa, string codModelo, int codContab)
        {
            var resp = new ErrorDto<List<PressModeloAjustes>>
            {
                Code = 0,
                Result = new List<PressModeloAjustes>()
            };

            const string proc = "[spPres_Modelo_Ajustes_Autorizados]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo
                };

                resp.Result = connection
                    .Query<PressModeloAjustes>(
                        proc,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Ajustes_Autorizados_SP: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Obtiene los usuarios autorizados de un modelo</summary>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_Autorizados_SP(int codEmpresa, string codModelo, int codContab)
        {
            var resp = new ErrorDto<List<PressModeloUsuarios>>
            {
                Code = 0,
                Result = new List<PressModeloUsuarios>()
            };

            const string proc = "[spPres_Modelo_Usuarios_Autorizados]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo
                };

                resp.Result = connection
                    .Query<PressModeloUsuarios>(
                        proc,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Usuarios_Autorizados_SP: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Obtiene los ajustes y usuarios de un modelo</summary>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_AjUs_Ajustes_SP(int codEmpresa, string codModelo, int codContab, string usuario)
        {
            var resp = new ErrorDto<List<PressModeloAjustes>>
            {
                Code = 0,
                Result = new List<PressModeloAjustes>()
            };

            const string proc = "[spPres_Modelo_AjUs_Ajustes]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo,
                    Usuario = usuario
                };

                resp.Result = connection
                    .Query<PressModeloAjustes>(
                        proc,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_AjUs_Ajustes_SP: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Obtiene los usuarios y ajustes de un modelo</summary>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_AjUs_Usuarios_SP(int codEmpresa, string codModelo, int codContab, string codAjuste)
        {
            var resp = new ErrorDto<List<PressModeloUsuarios>>
            {
                Code = 0,
                Result = new List<PressModeloUsuarios>()
            };

            const string proc = "[spPres_Modelo_AjUs_Usuarios]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new
                {
                    CodContab = codContab,
                    CodModelo = codModelo,
                    CodAjuste = codAjuste
                };

                resp.Result = connection
                    .Query<PressModeloUsuarios>(
                        proc,
                        parameters,
                        commandType: CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_AjUs_Usuarios_SP: " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>Ajuste Modelo (usuario-ajuste)</summary>
        public ErrorDto Pres_Modelo_AjUs_Registro_SP(int codEmpresa, PressModeloAjUsRegistro request)
        {
            var resp = new ErrorDto { Code = 0 };

            const string proc = "[spPres_Modelo_AjUs_Registro]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                int activoValue = request.Activo ? 1 : 0;

                var parameters = new
                {
                    CodContab = request.CodContab,
                    CodModelo = request.CodModelo,
                    CodAjuste = request.Cod_Ajuste,
                    Usuario = request.Usuario,
                    UsuarioReg = request.UsuarioReg,
                    Activo = activoValue
                };

                connection.Execute(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_AjUs_Registro_SP: " + ex.Message;
            }
            return resp;
        }

        /// <summary>Ajuste Modelo (ajustes)</summary>
        public ErrorDto Pres_Modelo_Ajustes_Registro_SP(int codEmpresa, PressModeloAjUsRegistro request)
        {
            var resp = new ErrorDto { Code = 0 };

            const string proc = "[spPres_Modelo_Ajustes_Registro]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                int activoValue = request.Activo ? 1 : 0;

                var parameters = new
                {
                    CodContab = request.CodContab,
                    CodModelo = request.CodModelo,
                    CodAjuste = request.Cod_Ajuste,
                    UsuarioReg = request.UsuarioReg,
                    Activo = activoValue
                };

                connection.Execute(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Ajustes_Registro_SP: " + ex.Message;
            }
            return resp;
        }

        /// <summary>Usuario Modelo Registro</summary>
        public ErrorDto Pres_Modelo_Usuarios_Registro_SP(int codEmpresa, PressModeloAjUsRegistro request)
        {
            var resp = new ErrorDto { Code = 0 };

            const string proc = "[spPres_Modelo_Usuarios_Registro]";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                int activoValue = request.Activo ? 1 : 0;

                var parameters = new
                {
                    CodContab = request.CodContab,
                    CodModelo = request.CodModelo,
                    Usuario = request.Usuario,
                    UsuarioReg = request.UsuarioReg,
                    Activo = activoValue
                };

                connection.Execute(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Usuarios_Registro_SP: " + ex.Message;
            }
            return resp;
        }

        /// <summary>Eliminar Modelo</summary>
        public ErrorDto Pres_Model_Eliminar(int codEmpresa, string codModelo)
        {
            var resp = new ErrorDto { Code = 0 };

            const string sql = @"
                DELETE FROM PRES_MODELOS
                WHERE COD_MODELO = @CodModelo;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                connection.Execute(sql, new { CodModelo = codModelo });

                resp.Description = "Modelo eliminado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Model_Eliminar: " + ex.Message;
            }
            return resp;
        }
    }
}