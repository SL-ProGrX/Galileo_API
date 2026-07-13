using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresModeloDb
    {
        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;

        public FrmPresModeloDb(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(stringConn);
        }

        private static int ToBit(bool value) => value ? 1 : 0;

        private ErrorDto<List<T>> ExecuteStoredProcList<T>(
            int codEmpresa,
            string procedureName,
            object? parameters,
            string metodoContexto)
        {
            var resp = new ErrorDto<List<T>>
            {
                Code = 0,
                Result = new List<T>()
            };

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection.Query<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = $"{metodoContexto}: {ex.Message}";
                resp.Result = null;
            }

            return resp;
        }

        private ErrorDto ExecuteStoredProcNonQuery(
            int codEmpresa,
            string procedureName,
            object? parameters,
            string metodoContexto,
            string successMessage = "Ok")
        {
            var resp = new ErrorDto { Code = 0, Description = successMessage };

            try
            {
                using var connection = CreateConnection(codEmpresa);

                connection.Execute(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = $"{metodoContexto}: {ex.Message}";
            }

            return resp;
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
                    Cod_Modelo = codModelo,
                    Cod_Conta = codContab
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
        public ErrorDto<PresModeloData> Pres_Modelo_scroll(
            int codEmpresa, int scrollValue, string? codModelo, int codContab)
        {
            var resp = new ErrorDto<PresModeloData> { Code = 0 };

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

                resp.Result = connection.QueryFirstOrDefault<PresModeloData>(
                    scrollValue == 1 ? sqlNext : sqlPrev,
                    new
                    {
                        CodContab = codContab,
                        CodModelo = codModelo ?? string.Empty  // <- clave para la primera vez
                    }
                );
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_scroll: " + ex.Message;
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
            const string proc = "[spPres_ModelosRegistra]";

            var estado = Strings.Mid(request.Estado, 1, 1); // Mantengo tu lógica VB

            var parameters = new
            {
                Cod_Modelo = request.Cod_Modelo,
                COD_CONTA = request.Cod_Contabilidad,
                Cierre = request.ID_Cierre,
                Descripcion = request.Descripcion,
                Notas = request.Notas,
                Estado = estado,
                Usuario = request.Usuario,
                Patrimonio = request.Patrimonio_Promedio
            };

            return ExecuteStoredProcNonQuery(
                codEmpresa,
                proc,
                parameters,
                "Pres_Modelo_Insertar",
                "Información guardada satisfactoriamente...");
        }

        /// <summary>Mapea Cuentas sin Centro Costo</summary>
        public ErrorDto Pres_MapeaCuentasSinCentroCosto_SP(int codEmpresa, string codModelo, int codContab, string usuario)
        {
            const string proc = "[spPres_MapeaCuentasSinCentroCosto]";

            var parameters = new
            {
                Modelo = codModelo,
                contabilidad = codContab,
                Usuario = usuario
            };

            return ExecuteStoredProcNonQuery(
                codEmpresa,
                proc,
                parameters,
                "Pres_MapeaCuentasSinCentroCosto_SP",
                "Revisión de Mapeo de Cuentas sin Centro de Costo, realizado satisfactoriamente!");
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
            var parameters = new
            {
                contabilidad = codContab,
                Modelo = codModelo
            };

            return ExecuteStoredProcList<PressModeloUsuarios>(
                codEmpresa,
                "[spPres_Modelo_Usuarios]",
                parameters,
                "Pres_Modelo_Usuarios_SP");
        }

        /// <summary>Obtiene los ajustes de un modelo</summary>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_SP(int codEmpresa, string codModelo, int codContab)
        {
            var parameters = new
            {
                Contabilidad = codContab,
                Modelo = codModelo
            };

            return ExecuteStoredProcList<PressModeloAjustes>(
                codEmpresa,
                "[spPres_Modelo_Ajustes]",
                parameters,
                "Pres_Modelo_Ajustes_SP");
        }

        /// <summary>Obtiene los ajustes y usuarios autorizados de un modelo</summary>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_Autorizados_SP(int codEmpresa, string codModelo, int codContab)
        {
            var parameters = new
            {
                Contabilidad = codContab,
                Modelo = codModelo
            };

            return ExecuteStoredProcList<PressModeloAjustes>(
                codEmpresa,
                "[spPres_Modelo_Ajustes_Autorizados]",
                parameters,
                "Pres_  Modelo_Ajustes_Autorizados_SP");
        }

        /// <summary>Obtiene los usuarios autorizados de un modelo</summary>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_Autorizados_SP(int codEmpresa, string codModelo, int codContab)
        {
            var parameters = new
            {
                Contabilidad = codContab,
                Modelo = codModelo
            };

            return ExecuteStoredProcList<PressModeloUsuarios>(
                codEmpresa,
                "[spPres_Modelo_Usuarios_Autorizados]",
                parameters,
                "Pres_Modelo_Usuarios_Autorizados_SP");
        }

        /// <summary>Obtiene los ajustes y usuarios de un modelo</summary>
        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_AjUs_Ajustes_SP(int codEmpresa, string codModelo, int codContab, string usuario)
        {
            var parameters = new
            {
                Contabilidad = codContab,
                Modelo = codModelo,
                Usuario = usuario
            };

            return ExecuteStoredProcList<PressModeloAjustes>(
                codEmpresa,
                "[spPres_Modelo_AjUs_Ajustes]",
                parameters,
                "Pres_Modelo_AjUs_Ajustes_SP");
        }

        /// <summary>Obtiene los usuarios y ajustes de un modelo</summary>
        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_AjUs_Usuarios_SP(int codEmpresa, string codModelo, int codContab, string codAjuste)
        {
            var parameters = new
            {
                Contabilidad = codContab,
                Modelo = codModelo,
                Ajuste = codAjuste
            };

            return ExecuteStoredProcList<PressModeloUsuarios>(
                codEmpresa,
                "[spPres_Modelo_AjUs_Usuarios]",
                parameters,
                "Pres_Modelo_AjUs_Usuarios_SP");
        }

        /// <summary>Ajuste Modelo (usuario-ajuste)</summary>
        public ErrorDto Pres_Modelo_AjUs_Registro_SP(int codEmpresa, PressModeloAjUsRegistro request)
        {
            var parameters = new
            {
                Contabilidad = request.CodContab,
                Modelo = request.CodModelo,
                Ajuste = request.Cod_Ajuste,
                Usuario = request.Usuario,
                UserReg = request.UsuarioReg,
                Activo = ToBit(request.Activo ?? false)
            };

            return ExecuteStoredProcNonQuery(
                codEmpresa,
                "[spPres_Modelo_AjUs_Registro]",
                parameters,
                "Pres_Modelo_AjUs_Registro_SP");
        }

        /// <summary>Ajuste Modelo (ajustes)</summary>
        public ErrorDto Pres_Modelo_Ajustes_Registro_SP(int codEmpresa, PressModeloAjUsRegistro request)
        {
            var parameters = new
            {
                Contabilidad = request.CodContab,
                Modelo = request.CodModelo,
                Ajuste = request.Cod_Ajuste,
                UserReg = request.UsuarioReg,
                Activo = ToBit(request.Activo ?? false)
            };

            return ExecuteStoredProcNonQuery(
                codEmpresa,
                "[spPres_Modelo_Ajustes_Registro]",
                parameters,
                "Pres_Modelo_Ajustes_Registro_SP");
        }

        /// <summary>Usuario Modelo Registro</summary>
        public ErrorDto Pres_Modelo_Usuarios_Registro_SP(int codEmpresa, PressModeloAjUsRegistro request)
        {
            var parameters = new
            {
                Contabilidad = request.CodContab,
                Modelo = request.CodModelo,
                Usuario = request.Usuario,
                UserReg = request.UsuarioReg,
                Activo = ToBit(request.Activo ?? false)
            };

            return ExecuteStoredProcNonQuery(
                codEmpresa,
                "[spPres_Modelo_Usuarios_Registro]",
                parameters,
                "Pres_Modelo_Usuarios_Registro_SP");
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

        /// <summary>
        /// Obtiene el rango de inicio y corte del cierre seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContab">Código de la contabilidad.</param>
        /// <param name="periodoId">Id del cierre o periodo seleccionado.</param>
        /// <returns>Información del cierre para construir los indicadores mensuales.</returns>
        public ErrorDto<PresModeloCierreData> Pres_Modelo_Cierre_Obtener(int codEmpresa, int codContab, int periodoId)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT TOP 1
                           INICIO_ANIO AS Inicio_Anio,
                           INICIO_MES  AS Inicio_Mes,
                           CORTE_ANIO  AS Corte_Anio,
                           CORTE_MES   AS Corte_Mes
                    FROM CNTX_CIERRES
                    WHERE COD_CONTABILIDAD = @CodContab
                      AND ID_CIERRE = @PeriodoId
                    ORDER BY INICIO_ANIO DESC;";

                return conn.QueryFirstOrDefault<PresModeloCierreData>(query,
                    new
                    {
                        CodContab = codContab,
                        PeriodoId = periodoId
                    }) ?? new PresModeloCierreData();
            });
        }

        /// <summary>
        /// Obtiene los indicadores mensuales registrados para un modelo y contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codModelo">Código del modelo.</param>
        /// <param name="codContab">Código de la contabilidad.</param>
        /// <returns>Lista de indicadores mensuales registrados.</returns>
        public ErrorDto<List<PresModeloIndicadorData>> Pres_Modelo_Indicadores_Obtener(int codEmpresa, string codModelo, int codContab)
        {
            var resp = new ErrorDto<List<PresModeloIndicadorData>>
            {
                Code = 0,
                Result = new List<PresModeloIndicadorData>()
            };

            const string sql = @"
        SELECT
            CORTE,
            COD_MODELO,
            COD_CONTABILIDAD,
            TIPO_CAMBIO,
            TASA_BASICA_PASIVA,
            INDICE_INFLACION,
            REGISTRO_USUARIO,
            REGISTRO_FECHA,
            MODIFICA_FECHA,
            MODIFICA_USUARIO
        FROM PRES_MODELOS_INDICADORES
        WHERE COD_MODELO = @CodModelo
          AND COD_CONTABILIDAD = @CodContab
        ORDER BY CORTE;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection.Query<PresModeloIndicadorData>(
                    sql,
                    new
                    {
                        CodModelo = codModelo,
                        CodContab = codContab
                    }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Indicadores_Obtener: " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Guarda los indicadores mensuales del modelo. Si el registro ya existe, lo actualiza; si no existe, lo inserta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de indicadores a guardar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Pres_Modelo_Indicadores_Guardar(int codEmpresa, PresModeloIndicadoresGuardarRequest request)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            const string sql = @"
        MERGE PRES_MODELOS_INDICADORES AS T
        USING (
            SELECT
                @Corte AS CORTE,
                @CodModelo AS COD_MODELO,
                @CodContab AS COD_CONTABILIDAD
        ) AS S
        ON T.CORTE = S.CORTE
        AND T.COD_MODELO = S.COD_MODELO
        AND T.COD_CONTABILIDAD = S.COD_CONTABILIDAD
        WHEN MATCHED THEN
            UPDATE SET
                T.TIPO_CAMBIO = @TipoCambio,
                T.TASA_BASICA_PASIVA = @TasaBasicaPasiva,
                T.INDICE_INFLACION = @IndiceInflacion,
                T.MODIFICA_FECHA = GETDATE(),
                T.MODIFICA_USUARIO = @Usuario
        WHEN NOT MATCHED THEN
            INSERT (
                CORTE,
                COD_MODELO,
                COD_CONTABILIDAD,
                TIPO_CAMBIO,
                TASA_BASICA_PASIVA,
                INDICE_INFLACION,
                REGISTRO_USUARIO,
                REGISTRO_FECHA
            )
            VALUES (
                @Corte,
                @CodModelo,
                @CodContab,
                @TipoCambio,
                @TasaBasicaPasiva,
                @IndiceInflacion,
                @Usuario,
                GETDATE()
            );";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                foreach (var item in request.Indicadores)
                {
                    connection.Execute(
                        sql,
                        new
                        {
                            Corte = item.Corte,
                            CodModelo = request.Cod_Modelo,
                            CodContab = request.Cod_Contabilidad,
                            TipoCambio = item.Tipo_Cambio,
                            TasaBasicaPasiva = item.Tasa_Basica_Pasiva,
                            IndiceInflacion = item.Indice_Inflacion,
                            Usuario = request.Usuario
                        });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelo_Indicadores_Guardar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Modelo Copiar un modelo a otro nuevo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Pres_Modelo_Copiar(PresModeloCopiar request)
        {
            using var connection = CreateConnection(request.cod_Empresa);
            try
            {
                
                var query = $@"spPres_W_Modelo_Usuarios_Copiar";

                var parametros = new
                {
                    cod_modelo_original = request.cod_Modelo_Origen,
                    cod_modelo_destino = request.cod_Modelo_Destino,
                    descripcion = request.descripcion,
                    usuario = request.usuario
                };

                connection.Execute(query, parametros, commandType: CommandType.StoredProcedure, commandTimeout: 600);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

    }
}