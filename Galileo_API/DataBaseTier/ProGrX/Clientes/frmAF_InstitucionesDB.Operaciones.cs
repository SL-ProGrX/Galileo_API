using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmAFInstitucionesDB
    {
        private const string SpInstitucionProcesoInicial = "spPrm_Institucion_Proceso_Inicial";
        private const string SpInstitucionCopia = "spAFI_Institucion_Copia";
        private const string MovimientoAplicaWeb = "Aplica - WEB";
        private const string MovimientoEliminaWeb = "Elimina - WEB";

        private const string SqlCambiarFechaCorte = @"
                    UPDATE dbo.instituciones
                    SET pr_fecha_corte = @FechaCorte
                    WHERE cod_institucion = @CodInstitucion;";

        private const string SqlCodigoDeduccionExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.AFI_INSTITUCIONES_CODIGOS
                    WHERE COD_INSTITUCION = @CodInstitucion
                      AND COD_DEDUCCION = @CodDeduccion;";

        private const string SqlCodigoDeduccionInsert = @"
                    INSERT INTO dbo.AFI_INSTITUCIONES_CODIGOS
                    (
                        COD_INSTITUCION,
                        COD_DEDUCCION,
                        descripcion,
                        activo,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @CodInstitucion,
                        @CodDeduccion,
                        @Descripcion,
                        @Activo,
                        GETDATE(),
                        @Usuario
                    );";

        private const string SqlCodigoDeduccionUpdate = @"
                    UPDATE dbo.AFI_INSTITUCIONES_CODIGOS
                    SET Descripcion = @Descripcion,
                        activo = @Activo
                    WHERE COD_INSTITUCION = @CodInstitucion
                      AND cod_deduccion = @CodDeduccion;";

        private const string SqlCodigoDeduccionDelete = @"
                    DELETE FROM dbo.AFI_INSTITUCIONES_CODIGOS
                    WHERE COD_DEDUCCION = @CodDeduccion
                      AND cod_institucion = @CodInstitucion;";

        private const string SqlAsignacionLineaInsert = @"
                    INSERT INTO dbo.AFI_INSTITUCION_ASIGNACION
                    (
                        cod_institucion,
                        cod_deduccion,
                        codigo,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @CodInstitucion,
                        @CodDeduccion,
                        @Codigo,
                        GETDATE(),
                        @Usuario
                    );";

        private const string SqlAsignacionLineaDelete = @"
                    DELETE FROM dbo.AFI_INSTITUCION_ASIGNACION
                    WHERE cod_institucion = @CodInstitucion
                      AND cod_deduccion = @CodDeduccion
                      AND codigo = @Codigo;";

        private const string SqlEmpresaVinculadaInsert = @"
                    INSERT INTO dbo.AFI_INSTITUCION_DEDUCTORA
                    (
                        COD_INSTITUCION,
                        COD_DEDUCTORA,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO
                    )
                    VALUES
                    (
                        @CodInstitucion,
                        @CodDeductora,
                        GETDATE(),
                        @Usuario
                    );";

        private const string SqlEmpresaVinculadaDelete = @"
                    DELETE FROM dbo.AFI_INSTITUCION_DEDUCTORA
                    WHERE cod_institucion = @CodInstitucion
                      AND cod_deductora = @CodDeductora;";

        private const string SqlInstitucionDelete = @"
                    DELETE FROM dbo.instituciones
                    WHERE cod_institucion = @CodInstitucion;";

        /// <summary>
        /// Cambiar fecha de corte de la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="FechaCorte"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_CambiarFecha(int CodEmpresa, int CodInstitucion, string FechaCorte, string Usuario)
        {
            if (!TryParseFechaCorte(FechaCorte, out var fechaFormateada))
            {
                return DbHelper.ErrorResponse("La fecha ingresada no es válida.", -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlCambiarFechaCorte,
                new
                {
                    CodInstitucion,
                    FechaCorte = fechaFormateada
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al cambiar fecha de corte.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Cambia Fecha de Corte Formalizaciones: {fechaFormateada} [Inst:{CodInstitucion}]", MovimientoAplicaWeb);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Inicializar fecha de deducción de la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Proceso"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_InicializarDeduccion(int CodEmpresa, int CodInstitucion, string Proceso, string Usuario)
        {
            var result = EjecutarStoredProcedure(
                CodEmpresa,
                SpInstitucionProcesoInicial,
                new
                {
                    CodInstitucion,
                    Proceso = NormalizarTexto(Proceso),
                    Usuario = NormalizarTexto(Usuario)
                },
                "Error al inicializar fecha de deducción.");

            if (result.Code == 0)
            {
                RegistrarBitacora(CodEmpresa, Usuario, $"Inicializa Fecha Corte para Deducciones: {NormalizarTexto(Proceso)} [Inst:{CodInstitucion}]", MovimientoAplicaWeb);
            }

            return result;
        }

        /// <summary>
        /// Guarda código, registra o actualiza según si existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Instituciones_Codigo_Guardar(int CodEmpresa, AfInstitucionesCodigosDto Info, string Usuario)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos del código de deducción son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => GuardarCodigoDeduccion(connection, Info, Usuario));
            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar código de deducción.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Cod. Deduc.: {NormalizarTexto(Info.cod_deduccion)} Inst.: {Info.cod_institucion}", result.Result ?? "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Eliminar código asociado a la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDeduccion"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Instituciones_Codigo_Eliminar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlCodigoDeduccionDelete,
                new
                {
                    CodInstitucion,
                    CodDeduccion = NormalizarTexto(CodDeduccion)
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar código de deducción.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Cod. Deduc.: {NormalizarTexto(CodDeduccion)} Inst.: {CodInstitucion}", MovimientoEliminaWeb);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Guardar vinculación o desvinculación de lineas de un código de institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDeduccion"></param>
        /// <param name="Codigo"></param>
        /// <param name="Checked"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Instituciones_Lineas_Asignacion_Guardar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Codigo, bool Checked, string Usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                Checked ? SqlAsignacionLineaInsert : SqlAsignacionLineaDelete,
                new
                {
                    CodInstitucion,
                    CodDeduccion = NormalizarTexto(CodDeduccion),
                    Codigo = NormalizarTexto(Codigo),
                    Usuario = NormalizarTexto(Usuario)
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar asignación de línea.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Inst. Asignación Código: {NormalizarTexto(CodDeduccion)} (Inst:{CodInstitucion}) Línea Crd:{NormalizarTexto(Codigo)}", Checked ? "Registra - WEB" : MovimientoEliminaWeb);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Guardar vinculación o desvinculación de empresa a la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDeductora"></param>
        /// <param name="Checked"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Empresas_Guardar(int CodEmpresa, int CodInstitucion, int CodDeductora, bool Checked, string Usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                Checked ? SqlEmpresaVinculadaInsert : SqlEmpresaVinculadaDelete,
                new
                {
                    CodInstitucion,
                    CodDeductora,
                    Usuario = NormalizarTexto(Usuario)
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar empresa vinculada.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Institución : {CodInstitucion} -> Deductora: {CodDeductora}", Checked ? MovimientoAplicaWeb : MovimientoEliminaWeb);
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Copiar institución, replica toda la información de una institución a una nueva
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CopiaDesc"></param>
        /// <param name="CopiaDescCorta"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Copiar(int CodEmpresa, int CodInstitucion, string CopiaDesc, string CopiaDescCorta, string Usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    SpInstitucionCopia,
                    new
                    {
                        CodInstitucion,
                        Destino = 0,
                        CopiaDesc = NormalizarTexto(CopiaDesc),
                        CopiaDescCorta = NormalizarTexto(CopiaDescCorta),
                        Usuario = NormalizarTexto(Usuario),
                        Copia1 = 1,
                        Copia2 = 1,
                        Copia3 = 1,
                        Copia4 = 1,
                        Copia5 = 1,
                        Copia6 = 1
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al copiar institución.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Copia de Institución: {CodInstitucion} -> Nueva -> [Inst:{result.Result}]", MovimientoAplicaWeb);
            return new ErrorDto { Code = result.Result, Description = "Ok" };
        }

        /// <summary>
        /// Eliminar institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Eliminar(int CodEmpresa, int CodInstitucion, string Usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstitucionDelete,
                new { CodInstitucion });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar institución.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, Usuario, $"Institución No.{CodInstitucion}", "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado sin retorno.
        /// </summary>
        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters, string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Guarda o actualiza un código de deducción.
        /// </summary>
        private string GuardarCodigoDeduccion(SqlConnection connection, AfInstitucionesCodigosDto info, string usuario)
        {
            var parametros = new
            {
                CodInstitucion = info.cod_institucion,
                CodDeduccion = NormalizarTexto(info.cod_deduccion),
                Descripcion = NormalizarTexto(info.descripcion),
                Activo = info.activo,
                Usuario = NormalizarTexto(usuario)
            };

            var existe = connection.QueryFirstOrDefault<int>(SqlCodigoDeduccionExiste, parametros);
            connection.Execute(existe == 0 ? SqlCodigoDeduccionInsert : SqlCodigoDeduccionUpdate, parametros);
            return existe == 0 ? "Registra - WEB" : "Modifica - WEB";
        }

        /// <summary>
        /// Valida y formatea la fecha de corte.
        /// </summary>
        private static bool TryParseFechaCorte(string fechaCorte, out string fechaFormateada)
        {
            fechaFormateada = string.Empty;
            if (!DateTime.TryParse(fechaCorte, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime fechaCorteDate)
                && !DateTime.TryParse(fechaCorte, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaCorteDate))
            {
                return false;
            }

            fechaFormateada = fechaCorteDate.ToString("yyyy/MM/dd");
            return true;
        }
    }
}