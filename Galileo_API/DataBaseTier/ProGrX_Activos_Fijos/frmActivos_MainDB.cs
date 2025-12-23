using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosMainDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        private const string MsgOk = "Ok";

        public FrmActivosMainDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mActivos        = new MActivosFijos(config);
            _portalDB        = new PortalDB(config);
        }

        #region Helpers privados

        private ErrorDto<List<T>> ExecuteListQuery<T>(
            int CodEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = new ErrorDto<List<T>>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = new List<T>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                result.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        private ErrorDto<T> ExecuteSingleQuery<T>(
            int CodEmpresa,
            string sql,
            T defaultValue,
            object? parameters = null)
        {
            var result = new ErrorDto<T>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = defaultValue
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var value = connection.Query<T>(sql, parameters).FirstOrDefault();
                result.Result = Equals(value, default(T)) ? defaultValue : value;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = defaultValue;
            }

            return result;
        }

        private void RegistrarBitacora(
            int CodEmpresa,
            string usuario,
            string detalle,
            string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId         = CodEmpresa,
                Usuario           = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento        = movimiento,
                Modulo            = vModulo
            });
        }

        #endregion

        /// <summary>
        /// Método para consultar los departamentos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Departamentos_Obtener(int CodEmpresa)
        {
            const string query = @"
                SELECT RTRIM(cod_departamento) AS item,
                       RTRIM(descripcion)      AS descripcion
                FROM Activos_departamentos
                ORDER BY cod_departamento;";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, query);
        }

        /// <summary>
        /// Método para consultar secciones según el departamento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Secciones_Obtener(
            int CodEmpresa,
            string departamento)
        {
            const string query = @"
                SELECT RTRIM(cod_Seccion)  AS item,
                       RTRIM(descripcion)  AS descripcion
                FROM Activos_Secciones
                WHERE cod_departamento = @departamento
                ORDER BY cod_Seccion;";

            return ExecuteListQuery<DropDownListaGenericaModel>(
                CodEmpresa,
                query,
                new { departamento });
        }

        /// <summary>
        /// Método para consultar los responsables según departamento y sección.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Responsable_Obtener(
            int CodEmpresa,
            string departamento,
            string seccion)
        {
            const string query = @"
                SELECT RTRIM(Identificacion) AS item,
                       RTRIM(Nombre)         AS descripcion
                FROM Activos_Personas
                WHERE cod_departamento = @departamento
                  AND cod_Seccion      = @seccion
                ORDER BY identificacion;";

            return ExecuteListQuery<DropDownListaGenericaModel>(
                CodEmpresa,
                query,
                new { departamento, seccion });
        }

        /// <summary>
        /// Método para consultar las localizaciones.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Localizacion_Obtener(int CodEmpresa)
        {
            const string query = @"
                SELECT RTRIM(COD_LOCALIZA) AS item,
                       RTRIM(descripcion)  AS descripcion
                FROM ACTIVOS_LOCALIZACIONES
                WHERE Activa = 1
                ORDER BY descripcion;";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, query);
        }

        /// <summary>
        /// Método para consultar los tipos de activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_TipoActivo_Obtener(int CodEmpresa)
        {
            const string query = @"
                SELECT RTRIM(tipo_activo) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM Activos_tipo_activo
                ORDER BY tipo_activo;";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, query);
        }

        /// <summary>
        /// Método para validar si permite el registro de un activo.
        /// </summary>
        public ErrorDto<int> Activos_Main_PermiteRegistros_Validar(int CodEmpresa)
        {
            const string query = @"
                SELECT ISNULL(REGISTRO_PERIODO_CERRADO,0) AS Permite
                FROM ACTIVOS_PARAMETROS;";

            return ExecuteSingleQuery<int>(CodEmpresa, query, 0);
        }

        /// <summary>
        /// Método para forzar un tipo de activo.
        /// </summary>
        public ErrorDto<int> Activos_Main_ForzarTipoActivo_Validar(int CodEmpresa)
        {
            const string query = @"
                SELECT FORZAR_TIPOACTIVO
                FROM Activos_parametros;";

            return ExecuteSingleQuery<int>(CodEmpresa, query, 0);
        }

        /// <summary>
        /// Método para consultar un número de placa (scroll simple).
        /// </summary>
        public ErrorDto<string> Activos_Main_NumeroPlaca_Consultar(
            int CodEmpresa,
            int orden,
            string placa)
        {
            string query = @"SELECT TOP 1 num_placa FROM Activos_Principal";

            if (orden == 1)
            {
                query += " WHERE num_placa > @placa ORDER BY num_placa ASC";
            }
            else
            {
                query += " WHERE num_placa < @placa ORDER BY num_placa ASC";
            }

            return ExecuteSingleQuery<string>(
                CodEmpresa,
                query,
                string.Empty,
                new { placa });
        }

        /// <summary>
        /// Método para consultar el histórico de un activo.
        /// </summary>
        public ErrorDto<List<MainHistoricoData>> Activos_Main_Historico_Consultar(
            int CodEmpresa,
            string codigo,
            string estadoHistorico)
        {
            const string query = @"
                EXEC spActivos_HistoricoConsolidado @codigo, @estadoHistorico;";

            return ExecuteListQuery<MainHistoricoData>(
                CodEmpresa,
                query,
                new { codigo, estadoHistorico });
        }

        /// <summary>
        /// Método para consultar el detalle de responsables de un activo.
        /// </summary>
        public ErrorDto<List<MainDetalleResponsablesData>> Activos_Main_DetalleResponsables_Consultar(
            int CodEmpresa,
            string placa)
        {
            const string query = @"
                SELECT R.Identificacion,
                       R.Nombre,
                       A.Registro_Fecha
                FROM Activos_Personas R
                INNER JOIN Activos_Responsables A
                    ON R.Identificacion = A.Identificacion
                WHERE A.num_placa = @placa
                ORDER BY A.registro_fecha DESC;";

            return ExecuteListQuery<MainDetalleResponsablesData>(
                CodEmpresa,
                query,
                new { placa });
        }

        /// <summary>
        /// Método para consultar datos de modificaciones de un activo.
        /// </summary>
        public ErrorDto<List<MainModificacionesData>> Activos_Main_Modificaciones_Consultar(
            int CodEmpresa,
            string placa)
        {
            const string query = @"
                SELECT X.id_AddRet,
                       X.fecha,
                       X.monto,
                       X.Descripcion,
                       RTRIM(J.cod_justificacion) + ' - ' + J.descripcion AS Justifica,
                       A.nombre,
                       CASE
                           WHEN X.Tipo = 'A' THEN 'Adicion/Mejora'
                           WHEN X.Tipo = 'M' THEN 'Mantenimiento'
                           WHEN X.Tipo = 'R' THEN 'Retiro'
                           WHEN X.Tipo = 'V' THEN 'Revaluación'
                           WHEN X.Tipo = 'D' THEN 'Deterioro'
                           ELSE ''
                       END AS TipoMov
                FROM Activos_retiro_adicion X
                INNER JOIN Activos_Principal A
                    ON X.num_placa = A.num_placa
                INNER JOIN Activos_justificaciones J
                    ON X.cod_justificacion = J.cod_justificacion
                LEFT JOIN Activos_proveedores P
                    ON X.compra_proveedor = P.cod_proveedor
                WHERE X.num_placa = @placa
                ORDER BY X.id_AddRet;";

            return ExecuteListQuery<MainModificacionesData>(
                CodEmpresa,
                query,
                new { placa });
        }

        /// <summary>
        /// Método para consultar la composición de un activo.
        /// </summary>
        public ErrorDto<List<MainComposicionData>> Activos_Main_Composicion_Consultar(
            int CodEmpresa,
            string placa)
        {
            const string query = @"
                SELECT num_placa,
                       'X' AS Tipo,
                       nombre AS descripcion,
                       depreciacion_periodo,
                       depreciacion_acum,
                       depreciacion_mes,
                       fecha_adquisicion AS Fecha,
                       Valor_historico   AS Libros
                FROM Activos_Principal
                WHERE num_placa = @placa

                UNION

                SELECT num_placa + '-' + CONVERT(char(3), id_AddRet) AS num_Placa,
                       'A' AS Tipo,
                       descripcion,
                       depreciacion_periodo,
                       depreciacion_acum,
                       depreciacion_mes,
                       fecha   AS Fecha,
                       Monto   AS Libros
                FROM Activos_retiro_Adicion
                WHERE tipo      = 'A'
                  AND num_placa = @placa
                ORDER BY Fecha ASC;";

            return ExecuteListQuery<MainComposicionData>(
                CodEmpresa,
                query,
                new { placa });
        }

        /// <summary>
        /// Método para consultar las pólizas de un activo.
        /// </summary>
        public ErrorDto<List<MainPolizasData>> Activos_Main_Polizas_Consultar(
            int CodEmpresa,
            string placa)
        {
            const string query = @"
                SELECT T.descripcion AS DescTipo,
                       P.num_poliza,
                       P.Documento,
                       P.fecha_Inicio,
                       P.fecha_vence,
                       P.Descripcion,
                       P.cod_poliza
                FROM Activos_polizas_tipos T
                INNER JOIN Activos_polizas P
                    ON T.tipo_poliza = P.tipo_poliza
                INNER JOIN Activos_polizas_asg A
                    ON P.cod_poliza = A.cod_poliza
                   AND A.num_placa = @placa
                ORDER BY P.fecha_vence DESC;";

            return ExecuteListQuery<MainPolizasData>(
                CodEmpresa,
                query,
                new { placa });
        }

        /// <summary>
        /// Método para consultar los datos generales de un activo.
        /// </summary>
        public ErrorDto<MainGeneralData> Activos_Main_DatosGenerales_Consultar(
            int CodEmpresa,
            string placa)
        {
            var result = new ErrorDto<MainGeneralData>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = new MainGeneralData()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT
                        A.num_placa,
                        A.PLACA_ALTERNA,
                        A.Nombre,
                        A.Tipo_activo,
                        A.met_depreciacion,
                        A.Vida_Util,
                        A.ud_produccion,
                        A.ud_anio,
                        A.Estado,
                        A.vida_util_en,
                        A.valor_historico,
                        A.valor_desecho,
                        A.fecha_adquisicion,
                        A.fecha_instalacion,
                        A.Descripcion,
                        A.Cod_Departamento,
                        A.Cod_Seccion,
                        A.Identificacion,
                        A.compra_documento,
                        A.COD_PROVEEDOR,
                        A.NUM_SERIE,
                        A.modelo,
                        A.marca,
                        A.otras_senas,
                        A.Registro_Usuario,
                        A.Registro_fecha,
                        A.depreciacion_periodo,
                        A.depreciacion_acum,
                        A.DEPRECIACION_MES,
                        RTRIM(D.descripcion)  AS Departamento_Desc,
                        RTRIM(S.descripcion)  AS Seccion_Desc,
                        RTRIM(R.Nombre)       AS Responsable_Desc,
                        ISNULL(P.descripcion,'N/A') AS Proveedor,
                        T.descripcion         AS Tipo_Activo_Desc,
                        ISNULL(A.cod_Localiza,'00')   AS Localiza_Id,
                        ISNULL(La.Descripcion,'No Indica') AS Localiza_Desc
                    FROM Activos_Principal A
                    INNER JOIN Activos_departamentos D
                        ON A.cod_departamento = D.cod_departamento
                    INNER JOIN Activos_Secciones S
                        ON A.cod_departamento = S.cod_departamento
                       AND A.cod_seccion      = S.cod_seccion
                    INNER JOIN Activos_Personas R
                        ON A.identificacion   = R.Identificacion
                    INNER JOIN Activos_proveedores P
                        ON A.cod_proveedor    = P.cod_proveedor
                    INNER JOIN Activos_tipo_activo T
                        ON A.tipo_activo      = T.tipo_activo
                    LEFT JOIN Activos_Localizaciones La
                        ON A.cod_localiza     = La.cod_localiza
                    WHERE A.num_placa = @placa;";

                result.Result = connection.Query<MainGeneralData>(
                    query,
                    new { placa }).FirstOrDefault() ?? new MainGeneralData();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        /// <summary>
        /// Método de validaciones para guardar.
        /// </summary>
        public ErrorDto<string> Activos_Main_Validaciones_Consultar(
            int CodEmpresa,
            string placa,
            string placaAlternativa)
        {
            var result = new ErrorDto<string>
            {
                Code        = -2,
                Description = MsgOk,
                Result      = string.Empty
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT COUNT(*) AS Existe
                    FROM Activos_Principal
                    WHERE Num_Placa = @placa;";

                int existe = connection.Query<int>(query, new { placa }).FirstOrDefault();

                if (existe > 0)
                {
                    result.Code   = existe;
                    result.Result = "El número de Placa para este activo ya Existe! ...";
                }
                else if (!string.IsNullOrWhiteSpace(placaAlternativa))
                {
                    const string query2 = @"
                        SELECT dbo.fxActivos_Registro_Valida_Placa_Alterna(@placa, @placaAlternativa) AS Resultado;";

                    int resultado2 = connection.Query<int>(
                        query2,
                        new { placa, placaAlternativa }).FirstOrDefault();

                    if (resultado2 == 0)
                        result.Code = existe;

                    result.Result = "El número de Placa Alterna ya está siendo utilizada por otro activo...";
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        /// <summary>
        /// Método para asignar responsables de un cambio / ingreso de un activo.
        /// </summary>
        private void Activos_Main_Responsable_Registrar(
            int CodEmpresa,
            string placa,
            string responsable,
            string usuario)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);

            const string query = @"EXEC spActivos_RegistroResponsable @placa, @responsable, @usuario;";

            connection.Execute(query, new { placa, responsable, usuario });
        }

        /// <summary>
        /// Método para registrar depreciación de un cambio / ingreso de un activo.
        /// </summary>
        private void Activos_Main_Depreciacion_Registrar(
            int CodEmpresa,
            string placa,
            string usuario,
            int limpia)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);

            const string query = @"EXEC spActivos_DepreciacionTabla @placa, @usuario, @limpia;";

            connection.Execute(query, new { placa, usuario, limpia });
        }

        /// <summary>
        /// Método para asignar asientos de un ingreso de un activo.
        /// </summary>
        private void Activos_Main_Asiento_Registrar(
            int CodEmpresa,
            string placa,
            string usuario)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);

            const string query = @"EXEC spActivos_AsientoRegistroInicial @placa, @usuario;";

            connection.Execute(query, new { placa, usuario });
        }

        /// <summary>
        /// Método para modificar un activo.
        /// </summary>
        public ErrorDto Activos_Main_Modificar(
            int CodEmpresa,
            MainGeneralData data,
            int aplicacionTotal,
            string usuario)
        {
            var result = new ErrorDto
            {
                Code        = 0,
                Description = MsgOk
            };

            try
            {
                DateTime mFechaUltCierre = _mActivos.fxActivos_FechaUltimoCierre(CodEmpresa);

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = @"
                    UPDATE Activos_Principal
                       SET nombre          = @nombre,
                           Placa_Alterna   = @placa_alterna,
                           descripcion     = @descripcion,
                           compra_documento = @compra_documento,
                           cod_proveedor   = @cod_proveedor,
                           num_serie       = @num_serie,
                           marca           = @marca,
                           modelo          = @modelo,
                           otras_senas     = @otras_senas";

                if (aplicacionTotal == 0)
                {
                    query += @",
                           tipo_activo        = @tipo_activo,
                           met_depreciacion   = @met_depreciacion,
                           Vida_Util_en       = @vida_util_en,
                           Vida_Util          = @vida_util,
                           UD_ANIO            = @ud_anio,
                           UD_PRODUCCION      = @ud_produccion,
                           valor_historico    = @valor_historico,
                           valor_desecho      = @valor_desecho,
                           fecha_adquisicion  = @fecha_adquisicion,
                           fecha_instalacion  = @fecha_instalacion,
                           cod_departamento   = @cod_departamento,
                           cod_seccion        = @cod_seccion,
                           identificacion     = @identificacion,
                           cod_Localiza       = @localiza_id,
                           Localiza_Fecha     = dbo.myGetdate(),
                           Modifica_Fecha     = GETDATE(),
                           Modifica_Usuario   = @usuario";
                }

                query += @"
                     WHERE num_placa = @num_placa;";

                connection.Execute(query, new
                {
                    data.nombre,
                    data.placa_alterna,
                    data.descripcion,
                    data.compra_documento,
                    data.cod_proveedor,
                    data.num_serie,
                    data.marca,
                    data.modelo,
                    data.otras_senas,
                    data.tipo_activo,
                    data.met_depreciacion,
                    data.vida_util_en,
                    data.vida_util,
                    data.ud_anio,
                    data.ud_produccion,
                    data.valor_historico,
                    data.valor_desecho,
                    data.fecha_adquisicion,
                    data.fecha_instalacion,
                    data.cod_departamento,
                    data.cod_seccion,
                    data.identificacion,
                    localiza_id = data.localiza_id,
                    usuario,
                    data.num_placa
                });

                if (data.fecha_adquisicion > mFechaUltCierre)
                {
                    Activos_Main_Depreciacion_Registrar(CodEmpresa, data.num_placa, usuario, 1);
                }

                if (data.depreciacion_acum == 0)
                {
                    Activos_Main_Responsable_Registrar(CodEmpresa, data.num_placa, data.identificacion, usuario);
                }

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Activo : {data.num_placa}",
                    "Modifica - WEB");
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Método para insertar un activo.
        /// </summary>
        public ErrorDto Activos_Main_Guardar(
            int CodEmpresa,
            MainGeneralData data,
            string usuario)
        {
            var result = new ErrorDto
            {
                Code        = 0,
                Description = MsgOk
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    INSERT INTO Activos_Principal
                        (num_placa, Placa_Alterna, nombre, tipo_activo, descripcion, met_depreciacion,
                         vida_util_en, vida_util, valor_historico, valor_desecho, fecha_adquisicion,
                         fecha_instalacion, cod_departamento, cod_seccion, identificacion, cod_localiza,
                         localiza_fecha, cod_proveedor, compra_documento, num_serie, marca, modelo,
                         otras_senas, estado, depreciacion_acum, depreciacion_mes, depreciacion_periodo,
                         ud_produccion, ud_anio, registro_fecha, registro_usuario)
                    VALUES
                        (@num_placa, @placa_alterna, @nombre, @tipo_activo, @descripcion, @met_depreciacion,
                         @vida_util_en, @vida_util, @valor_historico, @valor_desecho, @fecha_adquisicion,
                         @fecha_instalacion, @cod_departamento, @cod_seccion, @identificacion, @localiza_id,
                         dbo.myGetdate(), @cod_proveedor, @compra_documento, @num_serie, @marca, @modelo,
                         @otras_senas, 'A', 0, 0, 0, @ud_produccion, @ud_anio, dbo.myGetdate(), @usuario);";

                connection.Execute(query, new
                {
                    data.num_placa,
                    data.placa_alterna,
                    data.nombre,
                    data.tipo_activo,
                    data.descripcion,
                    data.met_depreciacion,
                    data.vida_util_en,
                    data.vida_util,
                    data.valor_historico,
                    data.valor_desecho,
                    data.fecha_adquisicion,
                    data.fecha_instalacion,
                    data.cod_departamento,
                    data.cod_seccion,
                    data.identificacion,
                    localiza_id = data.localiza_id,
                    data.cod_proveedor,
                    data.compra_documento,
                    data.num_serie,
                    data.marca,
                    data.modelo,
                    data.otras_senas,
                    data.ud_produccion,
                    data.ud_anio,
                    usuario
                });

                Activos_Main_Responsable_Registrar(CodEmpresa, data.num_placa, data.identificacion, usuario);
                Activos_Main_Depreciacion_Registrar(CodEmpresa, data.num_placa, usuario, 0);
                Activos_Main_Asiento_Registrar(CodEmpresa, data.num_placa, usuario);

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Activo : {data.num_placa}",
                    "Registra - WEB");
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Método para eliminar un número de placa.
        /// </summary>
        public ErrorDto Activos_Main_Eliminar(
            int CodEmpresa,
            string codigo,
            string usuario)
        {
            var result = new ErrorDto
            {
                Code        = 0,
                Description = MsgOk
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"EXEC spActivos_EliminaActivo @codigo, @usuario;";

                connection.Execute(query, new { codigo, usuario });

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Activo : {codigo}",
                    "Elimina - WEB");
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Método para consultar listado de activos disponibles para Adición o Retiro.
        /// </summary>
        public ErrorDto<List<ActivosData>> Activos_Main_Obtener(int CodEmpresa)
        {
            const string query = @"
                SELECT num_placa,
                       Placa_Alterna,
                       Nombre
                FROM Activos_Principal;";

            return ExecuteListQuery<ActivosData>(CodEmpresa, query);
        }

        /// <summary>
        /// Método para consultar parámetros de un tipo de activo.
        /// </summary>
        public ErrorDto<MainActivosTiposData> Activos_Main_TipoActivo_Consultar(
            int CodEmpresa,
            string tipo_activo)
        {
            var result = new ErrorDto<MainActivosTiposData>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = new MainActivosTiposData()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT MET_DEPRECIACION,
                           VIDA_UTIL,
                           TIPO_VIDA_UTIL
                    FROM Activos_tipo_activo
                    WHERE tipo_activo = @tipo_activo;";

                result.Result = connection.Query<MainActivosTiposData>(
                    query,
                    new { tipo_activo }).FirstOrDefault() ?? new MainActivosTiposData();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }

        /// <summary>
        /// Método para consultar la fecha del último cierre.
        /// </summary>
        public ErrorDto<DateTime> Activos_Main_FechaUltimoCierre(int CodEmpresa)
        {
            var result = new ErrorDto<DateTime>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = DateTime.Now
            };

            try
            {
                result.Result = _mActivos.fxActivos_FechaUltimoCierre(CodEmpresa);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Método para obtener el listado de proveedores.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Proveedores_Obtener(int CodEmpresa)
        {
            const string query = @"
                SELECT cod_proveedor AS item,
                       descripcion  AS descripcion
                FROM Activos_proveedores;";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, query);
        }

        /// <summary>
        /// Método para consultar el Id de placa inicial para un registro.
        /// </summary>
        public ErrorDto<string> Activos_Main_PlacaId_Consultar(int CodEmpresa)
        {
            const string query = @"SELECT dbo.fxActivos_Placa_Id() AS PLACA_ID;";

            return ExecuteSingleQuery<string>(CodEmpresa, query, string.Empty);
        }

        /// <summary>
        /// Método para consultar el listado de documentos de compra.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_DocCompas_Obtener(
            int CodEmpresa,
            string proveedor,
            DateTime adquisicion)
        {
            const string query = @"
                SELECT COD_FACTURA,
                       COD_PROVEEDOR,
                       CANTIDAD,
                       REGISTRADOS,
                       PRODUCTO
                FROM vCxP_Compras_Activos
                WHERE COD_PROVEEDOR = @proveedor
                  AND YEAR(FECHA)  = @anno
                  AND MONTH(FECHA) = @mes;";

            return ExecuteListQuery<DropDownListaGenericaModel>(
                CodEmpresa,
                query,
                new
                {
                    proveedor,
                    anno = adquisicion.Year,
                    mes  = adquisicion.Month
                });
        }
    }
}