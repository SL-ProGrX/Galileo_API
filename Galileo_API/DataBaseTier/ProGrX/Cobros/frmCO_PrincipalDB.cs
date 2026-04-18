using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using System.Data;


namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOPrincipalDB
    {
        private readonly PortalDB _portalDb;

        public FrmCOPrincipalDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Trae las operciones disponibles
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<OperacionBusquedaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                                SELECT TOP 10
                                    id_solicitud AS operacion,
                                    RTRIM(codigo) AS codigo,
                                    RTRIM(cedula) AS cedula,
                                    montoapr,
                                    saldo
                                FROM reg_creditos
                                WHERE estadosol = 'F'
                                ORDER BY id_solicitud
                            ";

                response.Result = cn.Query<OperacionBusquedaDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta la operacion 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>

        public ErrorDto<OperacionConsultarDto> Operacion_Consultar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<OperacionConsultarDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"SELECT 
                                rc.id_solicitud AS operacion,

                                CASE 
                                    WHEN rc.estadosol = 'F' THEN 'NORMAL'
                                    ELSE 'OTRO'
                                END AS descripcion,

                                CASE 
                                    WHEN rc.estadosol = 'F' THEN 'NO'
                                    ELSE 'SI'
                                END AS estado,

                     
                                s.cod_institucion AS codInstitucion,
                                ISNULL(rc.cod_deductora, s.cod_deductora) AS deductora,

                                c.codigo AS linea,
                                ISNULL(c.DESCRIPCION_LINEA, c.DESCRIPCION) AS lineaDescripcion,

                                rc.cedula AS identificacion,
                                RTRIM(s.nombre) AS identificacionDescripcion

                            FROM reg_creditos rc

                            LEFT JOIN socios s 
                                ON rc.cedula = s.cedula

                            LEFT JOIN catalogo c 
                                ON rc.codigo = c.codigo

                            WHERE rc.id_solicitud = @operacion";

                response.Result = cn.QueryFirstOrDefault<OperacionConsultarDto>(sql, new { operacion });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene las deductoras
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Deductoras_Listar(int codEmpresa, int codInstitucion)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                        SELECT 
                            COD_DEDUCTORA AS item,
                            DESCRIPCION   AS descripcion
                        FROM vAFI_Deductoras
                        WHERE cod_institucion = @codInstitucion
                        ORDER BY DESCRIPCION
                    ";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codInstitucion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los estados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<CoEstadoDto> Estado_Consultar(int codEmpresa, int operacion, DateTime? fechaCorte)
        {
            var response = new ErrorDto<CoEstadoDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var fecha = fechaCorte?.Date ?? DateTime.Today;

                const string sqlOperacion = @"
                 SELECT
                        CASE
                            WHEN rc.estado = 'C' THEN 'CANCELADO'
                            WHEN rc.proceso = 'J' THEN 'COBRO JUDICIAL'
                            WHEN rc.estadosol = 'F' THEN 'NORMAL'
                            ELSE ISNULL(RTRIM(rc.estado), '')
                        END AS estado,

                        ISNULL(rc.montoapr, 0) AS monto,
                        ISNULL(rc.plazo, 0) AS plazo,
                        ISNULL(rc.[int], 0) AS tasa1,
                        ISNULL(rc.interesv, 0) AS tasa2,
                        ISNULL(rc.cuota, 0) AS cuota,
                        ISNULL(rc.amortiza, 0) AS amortizado,
                        ISNULL(rc.interesc, 0) AS interes_pagado,
                        ISNULL(RTRIM(g.Descripcion), '') AS garantia,
                        ISNULL(RTRIM(rc.TDOCUMENTO), '') +
                        CASE
                            WHEN rc.nDocumento IS NULL
                              OR LTRIM(RTRIM(CONVERT(varchar(50), rc.nDocumento))) = ''
                            THEN ''
                            ELSE '-' + LTRIM(RTRIM(CONVERT(varchar(50), rc.nDocumento)))
                        END AS documento,
                        CONVERT(varchar(10), rc.prideduc, 120) AS primer_cuota,
                        CONVERT(varchar(10), rc.fecult, 120) AS ultima_cuota,
                        ISNULL(rc.saldo, 0) AS saldo
                    FROM reg_creditos rc
                    LEFT JOIN Crd_Garantia_Tipos g
                        ON rc.Garantia = g.Garantia
                    WHERE rc.id_solicitud = @operacion
                ";

                const string sqlMora = @"
                    EXEC spCbrCobroJudicialInteresesHoy @operacion, @fechaCorte
                ";

                var estado = cn.QueryFirstOrDefault<CoEstadoDto>(
                    sqlOperacion,
                    new { operacion });

                if (estado == null)
                {
                    response.Code = -1;
                    response.Description = "No se encontró la operación.";
                    return response;
                }

                var mora = cn.QueryFirstOrDefault(sqlMora, new
                {
                    operacion,
                    fechaCorte = fecha.ToString("yyyy/MM/dd")
                });

                if (mora != null)
                {
                    estado.antiguedad = mora.Antiguedad ?? string.Empty;
                    estado.interes_corriente = mora.RegIntCor ?? 0;
                    estado.interes_moratorio = mora.RegIntMor ?? 0;
                    estado.principal_atrasado = mora.RegPrincipal ?? 0;
                    estado.cargos = mora.Cargos ?? 0;
                    estado.polizas = mora.Poliza ?? 0;

                    estado.mora_financiera =
                        estado.interes_corriente +
                        estado.interes_moratorio +
                        estado.principal_atrasado +
                        estado.cargos +
                        estado.polizas;

                    estado.mora_legal =
                        estado.saldo +
                        estado.interes_corriente +
                        estado.interes_moratorio +
                        estado.cargos +
                        estado.polizas;

                    estado.total_deuda =
                        estado.saldo +
                        estado.interes_corriente +
                        estado.interes_moratorio +
                        estado.principal_atrasado +
                        estado.cargos +
                        estado.polizas;

                    estado.intereses_hoy =
                        estado.interes_corriente +
                        estado.interes_moratorio;

                    estado.fecha_corte = fecha;
                }
                else
                {
                    estado.antiguedad = string.Empty;
                    estado.interes_corriente = 0;
                    estado.interes_moratorio = 0;
                    estado.principal_atrasado = 0;
                    estado.cargos = 0;
                    estado.polizas = 0;
                    estado.mora_financiera = 0;
                    estado.mora_legal = estado.saldo;
                    estado.total_deuda = estado.saldo;
                    estado.intereses_hoy = 0;
                    estado.fecha_corte = fecha;
                }

                response.Result = estado;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene el historial
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CoHistorialDto>> Historial_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<CoHistorialDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT
                fecha,
                CASE
                    WHEN tipo = '01' THEN 'Traspaso de deudas'
                    WHEN tipo = '02' THEN 'Cobro Judicial'
                    WHEN tipo = '03' THEN 'Readecuaciones'
                    WHEN tipo = '04' THEN 'Arreglo de Pago'
                    WHEN tipo = '05' THEN 'Reversión Traspaso'
                    WHEN tipo = '06' THEN 'Reversión Cobro Judicial'
                    WHEN tipo = '07' THEN 'Registro de Incobrable'
                    WHEN tipo = '08' THEN 'Reversión de Incobrable'
                    WHEN tipo = '09' THEN 'Carta Primer Aviso'
                    WHEN tipo = '10' THEN 'Carta 2do y 3er. Aviso'
                    WHEN tipo = '11' THEN 'Activa Cobro a Fiadores'
                    WHEN tipo = '12' THEN 'Cancela Cobro a Fiadores'
                    ELSE ''
                END AS transaccion,
                ISNULL(usuario, '') AS usuario,
                ISNULL(notas, '') AS notas
            FROM cbr_historial
            WHERE id_solicitud = @operacion
            ORDER BY fecha DESC
        ";

                response.Result = cn.Query<CoHistorialDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene las gestiones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<COGestionDto>> Gestiones_Listar(int codEmpresa, string cedula)
        {
            var response = new ErrorDto<List<COGestionDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"SELECT 
                                            S.*,
                                            ISNULL(G.descripcion, '') AS gestion,
                                            ISNULL(C.DESCRIPCION, '') AS causa,
                                            ISNULL(A.descripcion, '') AS arreglo
                                        FROM CBR_Seguimiento S
                                        LEFT JOIN cbr_gestiones G 
                                            ON S.cod_gestion = G.cod_gestion
                                        LEFT JOIN CBR_CAUSAS_MOROSIDAD C 
                                            ON S.COD_CAUSA = C.COD_CAUSA
                                        LEFT JOIN CBR_TIPOS_ARREGLOS A 
                                            ON S.COD_ARREGLO = A.COD_ARREGLO
                                        WHERE S.cedula = @cedula
                                        ORDER BY S.cod_seg DESC";

                response.Result = cn.Query<COGestionDto>(
                    sql,
                    new { cedula }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los cobros fiadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<COCobroFiadorRowDto>> CobroFiadores_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<COCobroFiadorRowDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            EXEC spCbr_Cobro_Fiadores_List @operacion
        ";

                response.Result = cn.Query<COCobroFiadorRowDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Cancela el cobro de fiadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<string> CobroFiador_Cancelar(int codEmpresa, int operacion, string usuario)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = "exec spCbr_Cobro_Fiadores_Cancela @operacion, @usuario, ''";

                var result = cn.QueryFirstOrDefault(sql, new
                {
                    operacion,
                    usuario
                });

                if (result == null)
                {
                    response.Code = -1;
                    response.Description = "No se obtuvo respuesta del proceso.";
                    return response;
                }

                if (result.Pass == 1)
                {
                    response.Result = "Cancelación aplicada correctamente.";
                    return response;
                }

                response.Code = -1;
                response.Description = result.Mensaje ?? "No fue posible cancelar el cobro a fiador.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los traslados de deuda
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<COTrasladoDeudaRowDto>> TrasladoDeuda_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<COTrasladoDeudaRowDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT
                rc.id_solicitud AS operacion,
                RTRIM(rc.codigo) AS linea,
                RTRIM(rc.cedula) AS cedula,
                ISNULL(rc.montoapr, 0) AS monto,
                ISNULL(rc.saldo, 0) AS saldo,
                ISNULL(rc.cuota, 0) AS cuota,
                ISNULL(rc.interesv, 0) AS tasa,
                ISNULL(rc.plazo, 0) AS plazo,
                CAST(0 AS decimal(18,2)) AS interesPendiente,
                CAST(0 AS decimal(18,2)) AS cargos,
                CAST(0 AS decimal(18,2)) AS polizas,
                CASE
                    WHEN rc.estado = 'C' THEN 'CANCELADO'
                    WHEN rc.proceso = 'J' THEN 'COBRO JUDICIAL'
                    WHEN rc.proceso = 'T' THEN 'TRASPASO DEUDAS'
                    WHEN rc.estadosol = 'F' THEN 'NORMAL'
                    ELSE ISNULL(RTRIM(rc.estado), '')
                END AS estado,
                RTRIM(ISNULL(s.nombre, '')) AS nombre
            FROM reg_creditos rc
            LEFT JOIN socios s
                ON rc.cedula = s.cedula
            WHERE rc.referencia = @operacion
               OR rc.id_solicitud = @operacion
            ORDER BY rc.id_solicitud";

                response.Result = cn.Query<COTrasladoDeudaRowDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Revierte el traslado de Deuda
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<string> TrasladoDeuda_Revertir(int codEmpresa, COTrasladoDeudaRevertirRequestDto request)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            EXEC spCBR_TrasladoDeuda_Revertir
                 @operacion,
                 @usuario,
                 @nuevoMonto,
                 @plazo,
                 @tasa,
                 @tasaPts,
                 @operacionesSeleccionadasJson
        ";

                var operacionesSeleccionadasJson = System.Text.Json.JsonSerializer.Serialize(
                    request.operacionesseleccionadas
                );

                var result = cn.QueryFirstOrDefault(sql, new
                {
                    request.operacion,
                    request.usuario,
                    request.nuevomonto,
                    request.plazo,
                    request.tasa,
                    request.tasapts,
                    operacionesSeleccionadasJson
                });

                if (result == null)
                {
                    response.Code = -1;
                    response.Description = "No se obtuvo respuesta del proceso.";
                    return response;
                }

                if (result.Pass == 1)
                {
                    response.Result = result.Mensaje ?? "Reversión aplicada correctamente.";
                    return response;
                }

                response.Code = -1;
                response.Description = result.Mensaje ?? "No fue posible aplicar la reversión.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta de contacto
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<COContactoDto> Contacto_Consultar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<COContactoDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sqlContacto = @"
            SELECT
                RTRIM(s.cedula) AS identificacion,
                RTRIM(ISNULL(s.nombre, '')) AS nombre,
                'Deudor' AS calidad,
                '--' AS registro,

                LTRIM(RTRIM(
                    'PROVINCIA: ' + ISNULL(p.descripcion, '') + CHAR(13) + CHAR(10) +
                    'CANTON: ' + ISNULL(c.descripcion, '') + CHAR(13) + CHAR(10) +
                    'DISTRITO: ' + ISNULL(d.descripcion, '') + CHAR(13) + CHAR(10) +
                    'DIRECCION: ' + ISNULL(s.direccion, '')
                )) AS direccion,

                RTRIM(ISNULL(s.af_email, '')) AS email,
                '' AS apartado

            FROM reg_creditos rc
            INNER JOIN socios s ON rc.cedula = s.cedula

            LEFT JOIN provincias p ON s.provincia = p.cod_provincia
            LEFT JOIN cantones c ON s.provincia = c.cod_provincia AND s.canton = c.cod_canton
            LEFT JOIN distritos d ON s.provincia = d.cod_provincia AND s.canton = d.cod_canton AND s.distrito = d.cod_distrito

            WHERE rc.id_solicitud = @operacion

            UNION ALL
            SELECT
                RTRIM(f.cedulaf) AS identificacion,
                RTRIM(ISNULL(sf.nombre, '')) AS nombre,

                CASE 
                    WHEN f.calidad = 'F' THEN 'Fiador'
                    ELSE 'Codeudor'
                END AS calidad,

                RTRIM(ISNULL(e.descripcion, '')) AS registro,

                LTRIM(RTRIM(
                    'PROVINCIA: ' + ISNULL(p.descripcion, '') + CHAR(13) + CHAR(10) +
                    'CANTON: ' + ISNULL(c.descripcion, '') + CHAR(13) + CHAR(10) +
                    'DISTRITO: ' + ISNULL(d.descripcion, '') + CHAR(13) + CHAR(10) +
                    'DIRECCION: ' + ISNULL(sf.direccion, '')
                )) AS direccion,

                RTRIM(ISNULL(sf.af_email, '')) AS email,
                '' AS apartado

            FROM fiadores f
            INNER JOIN socios sf ON f.cedulaf = sf.cedula
            LEFT JOIN AFI_ESTADOS_PERSONA e ON sf.estadoActual = e.cod_Estado

            LEFT JOIN provincias p ON sf.provincia = p.cod_provincia
            LEFT JOIN cantones c ON sf.provincia = c.cod_provincia AND sf.canton = c.cod_canton
            LEFT JOIN distritos d ON sf.provincia = d.cod_provincia AND sf.canton = d.cod_canton AND sf.distrito = d.cod_distrito

            WHERE f.id_solicitud = @operacion
            AND f.estado = 'A'
        ";

                const string sqlTelefonos = @"
            SELECT
                RTRIM(ISNULL(tipo, '')) AS tipo,
                RTRIM(ISNULL(numero, '')) AS numero,
                RTRIM(ISNULL(ext, '')) AS ext,
                RTRIM(ISNULL(contacto, '')) AS contacto
            FROM Telefonos
            WHERE cedula = @cedula
            ORDER BY tipo, numero
        ";

                var contactos = cn.Query<COContactoItemDto>(
                    sqlContacto,
                    new { operacion }
                ).ToList();

                foreach (var item in contactos)
                {
                    item.telefonos = cn.Query<COContactoTelefonoDto>(
                        sqlTelefonos,
                        new { cedula = item.identificacion }
                    ).ToList();
                }

                response.Result = new COContactoDto
                {
                    contactos = contactos
                };
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// En lista las moras
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<COMoraDto>> Mora_Listar(int codEmpresa, int operacion, string tipo)
        {
            var response = new ErrorDto<List<COMoraDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                string sql = "";

                if (tipo == "P")
                {
                    sql = @"
            SELECT 
                CAST(FechaP AS VARCHAR(10)) AS proceso,
                ISNULL(FecUlt, GETDATE()) AS fecha,

                ISNULL(IntC,0) AS intCor,
                ISNULL(IntM,0) AS intMor,
                ISNULL(Cargo,0) AS cargo,
                0 AS poliza,
                ISNULL(Amortiza,0) AS principal,

                '' AS tipo,
                '' AS ncon,
                '' AS concepto,
                '' AS usuario
            FROM Morosidad
            WHERE id_solicitud = @operacion 
              AND Estado = 'A'
            ORDER BY FechaP DESC";
                }
                else if (tipo == "C")
                {
                    sql = @"
            SELECT 
                CAST(Proceso AS VARCHAR(10)) AS proceso,
                Fecha AS fecha,

                ISNULL(IntCor,0) AS intCor,
                ISNULL(IntMor,0) AS intMor,
                ISNULL(Cargo,0) AS cargo,
                ISNULL(Poliza,0) AS poliza,
                ISNULL(Principal,0) AS principal,

                ISNULL(Tipo,'') AS tipo,
                ISNULL(Ncon,'') AS ncon,
                ISNULL(Concepto,'') AS concepto,
                ISNULL(Usuario,'') AS usuario
            FROM vCRDsReportesMov
            WHERE id_solicitud = @operacion
            ORDER BY Fecha DESC";
                }
                else // T
                {
                    sql = @"
            SELECT 
                CAST(Proceso AS VARCHAR(10)) AS proceso,
                Fecha AS fecha,

                ISNULL(IntCor,0) AS intCor,
                ISNULL(IntMor,0) AS intMor,
                ISNULL(Cargo,0) AS cargo,
                ISNULL(Poliza,0) AS poliza,
                ISNULL(Principal,0) AS principal,

                ISNULL(Tipo,'') AS tipo,
                ISNULL(Ncon,'') AS ncon,
                ISNULL(Concepto,'') AS concepto,
                ISNULL(Usuario,'') AS usuario
            FROM vCRDsReportesMov
            WHERE id_solicitud = @operacion

            UNION ALL

            SELECT 
                CAST(FechaP AS VARCHAR(10)) AS proceso,
                ISNULL(FecUlt, GETDATE()) AS fecha,

                ISNULL(IntC,0) AS intCor,
                ISNULL(IntM,0) AS intMor,
                ISNULL(Cargo,0) AS cargo,
                0 AS poliza,
                ISNULL(Amortiza,0) AS principal,

                '' AS tipo,
                '' AS ncon,
                '' AS concepto,
                '' AS usuario
            FROM Morosidad
            WHERE id_solicitud = @operacion 
              AND Estado = 'A'

            ORDER BY fecha DESC";
                }

                var data = cn.Query<COMoraDto>(sql, new { operacion }).ToList();

                response.Result = data;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los ejecutivos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<COEjecutivoDto>> Ejecutivos_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<COEjecutivoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT
                fecha_asignacion AS fecha,
                UPPER(RTRIM(usuario)) AS oficial,
                ISNULL(mantener, 0) AS mantiene,
                ISNULL(rebajo_doble, 0) AS rebajo,
                ISNULL(aplica_mora, 0) AS dobleMora
            FROM cbr_asignacion_h
            WHERE cedula = (
                SELECT cedula 
                FROM reg_creditos 
                WHERE id_solicitud = @operacion
            )
            ORDER BY fecha_asignacion DESC
        ";

                response.Result = cn.Query<COEjecutivoDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        ///  En lista las lineas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                RTRIM(codigo) AS item,
                RTRIM(ISNULL(DESCRIPCION_LINEA, DESCRIPCION)) AS descripcion
            FROM catalogo
            ORDER BY descripcion
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        ///  En lista las personas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Personas_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                RTRIM(cedula) AS item,
                RTRIM(nombre) AS descripcion
            FROM socios
            ORDER BY nombre
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Lista las lineas por persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> LineasPorPersona_Listar(int codEmpresa, string cedula)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT DISTINCT
                RTRIM(c.codigo) AS item,
                RTRIM(ISNULL(c.DESCRIPCION_LINEA, c.DESCRIPCION)) AS descripcion
            FROM reg_creditos rc
            INNER JOIN catalogo c
                ON rc.codigo = c.codigo
            WHERE rc.cedula = @cedula
            ORDER BY descripcion
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { cedula }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Lista las operaciones persona por liena
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto<List<OperacionBusquedaDto>> OperacionesPorPersonaLinea_Listar(int codEmpresa, string cedula, string linea)
        {
            var response = new ErrorDto<List<OperacionBusquedaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT
                rc.id_solicitud AS operacion,
                RTRIM(rc.codigo) AS codigo,
                RTRIM(rc.cedula) AS cedula,
                ISNULL(rc.montoapr, 0) AS montoapr,
                ISNULL(rc.saldo, 0) AS saldo
            FROM reg_creditos rc
            WHERE rc.cedula = @cedula
              AND rc.codigo = @linea
            ORDER BY rc.id_solicitud DESC
        ";

                response.Result = cn.Query<OperacionBusquedaDto>(
                    sql,
                    new { cedula, linea }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Cambia la deductora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="deductora"></param>
        /// <returns></returns>
        public ErrorDto<string> CambiarDeductora(int codEmpresa, int operacion, int deductora)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"UPDATE reg_creditos
                                SET cod_deductora = @deductora
                                WHERE id_solicitud = @operacion ";

                var rows = cn.Execute(sql, new
                {
                    operacion,
                    deductora
                });

                if (rows == 0)
                {
                    response.Code = -1;
                    response.Description = "No se pudo actualizar la deductora.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Valida el congelmiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ErrorDto<bool> ValidarCongelamiento(int codEmpresa, string cedula, string tipo)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                string columna = tipo switch
                {
                    "per_traspaso_deudas" => "PER_TRASPASO_DEUDAS",
                    "per_cobro_judicial" => "PER_COBRO_JUDICIAL",
                    "per_reversiones" => "PER_REVERSIONES",
                    "per_readecuaciones" => "PER_READECUACIONES",
                    _ => throw new ArgumentException("Tipo no válido", nameof(tipo))
                };

                string sql = $@"
            SELECT COUNT(1)
            FROM afi_congelar
            WHERE estado = 'A'
              AND cedula = @cedula
              AND dbo.MyGetdate() BETWEEN fecha_inicia AND fecha_finaliza
              AND {columna} = 0";

                var count = cn.ExecuteScalar<int>(sql, new { cedula });

                response.Result = count > 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Valida el paso de cobro judicial
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<bool> ValidarPasoCobroJudicial(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT COUNT(1)
            FROM reg_Creditos R
            INNER JOIN catalogo C ON R.codigo = C.codigo
                AND C.retencion = 'N'
                AND C.poliza = 'N'
            WHERE R.id_solicitud = @operacion
              AND R.Proceso <> 'J'";

                var count = cn.ExecuteScalar<int>(sql, new { operacion });

                response.Result = count == 1;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Ejecuta el proceso de cobro judicial
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<string> CobroJudicial_Ejecutar(int codEmpresa, int operacion, string usuario)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var parameters = new
                {
                    operacion,
                    usuario
                };

                cn.Execute(
                    "spCO_CobroJudicial_Ejecutar",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                response.Result = "Operación enviada a cobro judicial correctamente";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

       /// <summary>
       /// Lista los avisos
       /// </summary>
       /// <param name="codEmpresa"></param>
       /// <param name="operacion"></param>
       /// <returns></returns>
        public ErrorDto<List<COAvisoDto>> Avisos_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<COAvisoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
            SELECT 
                fecha = fecha_aviso,
                tipo = CASE 
                        WHEN tipo_aviso = 1 THEN 'Primer Aviso'
                        WHEN tipo_aviso = 2 THEN 'Segundo Aviso'
                        ELSE 'Otro Aviso'
                       END
            FROM cbr_avisos
            WHERE id_solicitud = @operacion
            ORDER BY fecha_aviso
        ";

                response.Result = cn.Query<COAvisoDto>(
                    sql,
                    new { operacion }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    }
}

