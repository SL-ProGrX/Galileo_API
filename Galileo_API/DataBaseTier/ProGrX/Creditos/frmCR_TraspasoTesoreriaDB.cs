using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public class FrmCRTraspasoTesoreriaDB
    {
        private const string RemesaEstadoSql = "select estado from CRD_REMESAS_TES where cod_remesa = @cod_remesa";
        private readonly int vModulo = 3; // Modulo de Créditos
        private readonly MTesoreria _mtes;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        public FrmCRTraspasoTesoreriaDB(IConfiguration config)
        {
            var appConfig = config ?? throw new ArgumentNullException(nameof(config));
            _mtes = new MTesoreria(appConfig);
            _portalDB = new PortalDB(appConfig);
            _Security_MainDB = new MSecurityMainDb(appConfig);
        }

        #region remesas

        /// <summary>
        /// Obtiene las ultimas 50 remesas registradas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<RemesaModel>> Cr_TraspasoTes_Remesas_Listar(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<RemesaModel>(
                _portalDB,
                CodEmpresa,
                @"select TOP 50 T.cod_remesa, T.usuario, T.fecha, T.estado, 
                         T.fecha_inicio, T.fecha_corte, T.notas,
                         isnull(D.Casos,0) as casos, isnull(D.Monto,0) as monto
                  from CRD_REMESAS_TES T 
                  left join vCrd_Remesa_Tes_Rsm D on T.cod_Remesa = D.cod_Remesa
                  order by T.fecha desc");
        }

        /// <summary>
        /// Obtiene una remesa por su codigo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_remesa"></param>
        /// <returns></returns>
        public ErrorDto<RemesaModel> Cr_TraspasoTes_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var remesa = connection.QueryFirstOrDefault<RemesaModel>(
                    @"select T.cod_remesa, T.usuario, T.fecha, T.estado, 
                             T.fecha_inicio, T.fecha_corte, T.notas,
                             isnull(D.Casos,0) as casos, isnull(D.Monto,0) as monto
                      from CRD_REMESAS_TES T 
                      left join vCrd_Remesa_Tes_Rsm D on T.cod_Remesa = D.cod_Remesa
                      where T.cod_remesa = @cod_remesa",
                    new { cod_remesa });

                if (remesa is null)
                {
                    return DbHelper.CreateErrorResponse("No se encontró la remesa.", -1, new RemesaModel());
                }

                return DbHelper.CreateOkResponse(remesa);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al obtener la remesa: {ex.Message}", -1, new RemesaModel());
            }
        }

        /// <summary>
        /// Crea una nueva remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<RemesaModel> Cr_TraspasoTes_Remesa_Crear(int CodEmpresa, RemesaRequest request, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var nuevoCod = connection.QueryFirstOrDefault<int>(
                    "select isnull(max(cod_remesa),0) + 1 from CRD_REMESAS_TES");

                connection.Execute(
                    @"insert CRD_REMESAS_TES(cod_remesa, usuario, fecha, estado, fecha_inicio, fecha_corte, notas)
                      values(@cod_remesa, @usuario, dbo.MyGetdate(), 'A', @fecha_inicio, @fecha_corte, @notas)",
                    new
                    {
                        cod_remesa = nuevoCod,
                        usuario,
                        request.fecha_inicio,
                        request.fecha_corte,
                        request.notas
                    });

                RegistrarBitacora(CodEmpresa, usuario, $"Remesa de CRD Traslado a Tesoreria : {nuevoCod}", "Registra - WEB");

                var remesa = connection.QueryFirstOrDefault<RemesaModel>(
                    "select * from CRD_REMESAS_TES where cod_remesa = @cod_remesa",
                    new { cod_remesa = nuevoCod });

                return DbHelper.CreateOkResponse(remesa ?? new RemesaModel());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al crear la remesa: {ex.Message}", -1, new RemesaModel());
            }
        }

        /// <summary>
        /// Actualiza una remesa existente (solo si esta Abierta)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTes_Remesa_Modificar(int CodEmpresa, RemesaRequest request, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var estado = connection.QueryFirstOrDefault<string?>(
                    RemesaEstadoSql,
                    new { request.cod_remesa });

                if (estado != "A")
                {
                    return DbHelper.ErrorResponse("No se puede modificar la remesa, porque ya fue cerrada.", -1);
                }

                connection.Execute(
                    @"update CRD_REMESAS_TES 
                      set usuario = @usuario, fecha_inicio = @fecha_inicio, 
                          fecha_corte = @fecha_corte, notas = @notas
                      where cod_remesa = @cod_remesa",
                    new
                    {
                        request.cod_remesa,
                        usuario,
                        request.fecha_inicio,
                        request.fecha_corte,
                        request.notas
                    });

                RegistrarBitacora(CodEmpresa, usuario, $"Remesa de CRD Traslado a Tesoreria : {request.cod_remesa}", "Modifica - WEB");

                return DbHelper.OkResponse("Remesa modificada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al modificar la remesa: {ex.Message}", -1);
            }
        }

        /// <summary>
        /// Elimina una remesa (solo si esta Abierta)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_remesa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTes_Remesa_Eliminar(int CodEmpresa, int cod_remesa, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var estado = connection.QueryFirstOrDefault<string?>(
                    RemesaEstadoSql,
                    new { cod_remesa });

                if (estado != "A")
                {
                    return DbHelper.ErrorResponse("No se puede eliminar la remesa, porque ya fue cerrada.", -1);
                }

                connection.Execute(
                    "delete CRD_REMESAS_TES_detalle where Cod_Remesa = @cod_remesa",
                    new { cod_remesa });

                connection.Execute(
                    "delete CRD_REMESAS_TES where Cod_Remesa = @cod_remesa",
                    new { cod_remesa });

                RegistrarBitacora(CodEmpresa, usuario, $"Remesa de CRD Traslado a Tesoreria : {cod_remesa}", "Elimina - WEB");

                return DbHelper.OkResponse("Remesa eliminada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al eliminar la remesa: {ex.Message}", -1);
            }
        }

        #endregion

        #region cargar

        /// <summary>
        /// Obtiene las remesas en estado Abierta para el combo de carga
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_RemesasAbiertas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                @"select cod_remesa as item,
                         CONCAT(cod_remesa,' - ',USUARIO,' - ', FECHA, ' I:', CONVERT(varchar(10),FECHA_INICIO,103), ' C:', CONVERT(varchar(10),FECHA_CORTE,103)) as descripcion
                  from CRD_REMESAS_TES
                  where estado = 'A'
                  order by fecha desc");
        }

        /// <summary>
        /// Obtiene las operaciones disponibles para cargar en una remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_remesa"></param>
        /// <returns></returns>
        public ErrorDto<List<CargaOperacionModel>> Cr_TraspasoTes_Carga_Buscar(int CodEmpresa, int cod_remesa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var remesa = connection.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)?>(
                   "select fecha_inicio, fecha_corte from CRD_REMESAS_TES where cod_remesa = @cod_remesa",
                   new { cod_remesa });

                if (!remesa.HasValue)
                {
                    return DbHelper.CreateErrorResponse("No se encontró la remesa.", -1, new List<CargaOperacionModel>());
                }

                var fechaInicio = remesa.Value.fecha_inicio.Date;
                var fechaCorte = remesa.Value.fecha_corte.Date.AddDays(1).AddTicks(-1);

                var lista = connection.Query<CargaOperacionModel>(
                    @"select R.id_solicitud, R.codigo, S.cedula, S.nombre,
                             R.montoapr, R.monto_girado,
                             isnull(vD.Numero,0) as desem_num, isnull(vD.Monto,0) as desem_monto,
                             (R.monto_girado + isnull(vD.Monto,0)) as total,
                             case when exists(
                                select 1 from reg_creditos R2
                                where R2.cedula = R.cedula and R2.monto_girado = R.monto_girado
                                  and R2.id_solicitud <> R.id_solicitud
                                  and R2.tesoreria is not null
                             ) then 1 else 0 end as duplicado
                      from reg_creditos R
                      inner join Socios S on R.cedula = S.cedula
                      inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
                      left join CRD_REMESAS_TES_DETALLE Td on R.id_solicitud = Td.id_solicitud
                      left join vCrdOperacion_DesembolsosGiro vD on R.id_solicitud = vD.id_solicitud
                      where R.estadosol = 'F'
                        and R.fechaforp between @fechaInicio and @fechaCorte
                        and R.tesoreria is null
                        and R.estado in('A','C')
                        and Td.id_solicitud is null
                        and (R.Emitir in('CK','TE') or isnull(vD.Monto,0) > 0)
                      order by R.id_solicitud",
                    new { fechaInicio, fechaCorte }).ToList();

                return DbHelper.CreateOkResponse(lista);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al consultar operaciones para carga a remesa: {ex.Message}", -1, new List<CargaOperacionModel>());
            }

        }

        /// <summary>
        /// Carga operaciones seleccionadas a una remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTes_Carga_Ejecutar(int CodEmpresa, CargaRequest request, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var estado = connection.QueryFirstOrDefault<string?>(
                    RemesaEstadoSql,
                    new { request.cod_remesa });

                if (estado != "A")
                {
                    return DbHelper.ErrorResponse("La remesa actual ya se encuentra cerrada.", -1);
                }

                foreach (var idSolicitud in request.operaciones)
                {
                    var operacion = connection.QueryFirstOrDefault<(decimal monto_girado, decimal desem_monto)?>(
                        @"select R.monto_girado, isnull(vD.Monto,0) as desem_monto
                          from reg_creditos R
                          left join vCrdOperacion_DesembolsosGiro vD on R.id_solicitud = vD.id_solicitud
                          where R.id_solicitud = @id_solicitud",
                        new { idSolicitud });

                    if (operacion.HasValue)
                    {
                        connection.Execute(
                            @"insert CRD_REMESAS_TES_DETALLE(cod_remesa, id_solicitud, monto, desembolsos)
                              values(@cod_remesa, @id_solicitud, @monto, @desembolsos)",
                            new
                            {
                                request.cod_remesa,
                                id_solicitud = idSolicitud,
                                monto = operacion.Value.monto_girado,
                                desembolsos = operacion.Value.desem_monto
                            });
                    }
                }

                RegistrarBitacora(CodEmpresa, usuario, $"Carga Remesa Traslado a Tesoreria : {request.cod_remesa}", "Aplica - WEB");

                return DbHelper.OkResponse("Proceso realizado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al cargar operaciones a la remesa: {ex.Message}", -1);
            }
        }

        /// <summary>
        /// Cierra una remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_remesa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTes_Remesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var estado = connection.QueryFirstOrDefault<string?>(
                    RemesaEstadoSql,
                    new { cod_remesa });

                if (estado != "A")
                {
                    return DbHelper.ErrorResponse("La remesa actual ya se encuentra cerrada.", -1);
                }

                connection.Execute(
                    "update CRD_REMESAS_TES set estado = 'C' where cod_remesa = @cod_remesa",
                    new { cod_remesa });

                RegistrarBitacora(CodEmpresa, usuario, $"Cierra Remesa Traslado a Tesoreria : {cod_remesa}", "Aplica - WEB");

                return DbHelper.OkResponse("Remesa cerrada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al cerrar la remesa: {ex.Message}", -1);
            }
        }

        #endregion

        #region trasladar

        /// <summary>
        /// Método para obtener las remesas en estado 'C' (Cerradas) para el traspaso a tesoreria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                @"select cod_remesa as item,
                         CONCAT(cod_remesa,' - ',FECHA_INICIO,' - ', FECHA_CORTE, ' - ', USUARIO ) as descripcion
                  from CRD_REMESAS_TES
                  where estado = 'C'
                  order by fecha desc");
        }

        /// <summary>
        /// Método: Obtiene los tokens disponibles para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// Método: Genera un nuevo token para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, usuario);
        }

        public ErrorDto<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var remesa = connection.QueryFirstOrDefault<(DateTime fecha_inicio, DateTime fecha_corte)?>(
                    "select fecha_inicio, fecha_corte from CRD_REMESAS_TES where cod_remesa = @cod_remesa",
                    new { cod_remesa });

                if (!remesa.HasValue)
                {
                    return DbHelper.CreateErrorResponse("No se encontró la remesa.", -1, new List<TraspasoModel>());
                }

                var fechaInicio = remesa.Value.fecha_inicio.Date;
                var fechaCorte = remesa.Value.fecha_corte.Date.AddDays(1).AddTicks(-1);

                var lista = connection.Query<TraspasoModel>(
                    @"select R.id_solicitud,
                             R.codigo,
                             S.cedula,
                             S.nombre,
                             R.montoapr,
                             R.monto_girado,
                             isnull(D.Numero,0) as Desembolsos_Numero,
                             isnull(D.Monto,0) as Desembolsos
                      from reg_creditos R
                      inner join Socios S on R.cedula = S.cedula
                      inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
                      left join vCrdOperacion_DesembolsosGiro D on R.id_Solicitud = D.id_Solicitud
                      where R.estadosol = 'F'
                        and R.fechaforp between @fechaInicio and @fechaCorte
                        and R.estado in('A','C')
                        and R.id_solicitud in(
                            select id_solicitud
                            from CRD_REMESAS_TES_DETALLE
                            where cod_remesa = @id_remesa)
                      order by R.id_solicitud",
                    new
                    {
                        fechaInicio,
                        fechaCorte,
                        id_remesa = cod_remesa
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al consultar operaciones para traslado a tesorería: {ex.Message}", -1, new List<TraspasoModel>());
            }
        }

        public ErrorDto CrTraspasoTes_Traslado_Generar(int CodEmpresa, int cod_remesa, string usuario, string? token)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var tokenTrabajo = NormalizarToken(token);
                if (string.IsNullOrWhiteSpace(tokenTrabajo))
                {
                    tokenTrabajo = ObtenerOTokenGenerado(CodEmpresa, usuario, connection);
                }

                var lista = connection.Query<(int id_solicitud, string codigo)>(
                    @"select id_solicitud, codigo
                      from reg_creditos
                      where estado in('A','C')
                        and estadosol = 'F'
                        and tesoreria is null
                        and id_solicitud in(
                            select id_solicitud
                            from CRD_REMESAS_TES_DETALLE
                            where cod_remesa = @cod_remesa)",
                    new { cod_remesa }).ToList();

                foreach (var item in lista)
                {
                    connection.Execute(
                        "exec spCrdCreditoEnviaTesoreria_Todo @Operacion, @Token, @Remesa, @RemesaTipo",
                        new
                        {
                            Operacion = item.id_solicitud,
                            Token = tokenTrabajo,
                            Remesa = cod_remesa,
                            RemesaTipo = "CRD"
                        });

                    RegistrarBitacora(
                        CodEmpresa,
                        usuario,
                        $"Traspaso a Tesoreria de la Operacion y Desembol OP: {item.id_solicitud}",
                        "Registra - WEB");

                    _mtes.sbCrdOperacionTags(
                        CodEmpresa,
                        item.id_solicitud,
                        item.codigo,
                        "S04",
                        usuario,
                        string.Empty,
                        $"Remesa de Traslado No..: {cod_remesa}");
                }

                connection.Execute(
                    "update CRD_REMESAS_TES SET Estado = 'T' Where cod_remesa = @cod_remesa",
                    new { cod_remesa });

                return DbHelper.OkResponse("Operaciones Enviadas a Tesoreria Satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al generar traspaso a tesorería: {ex.Message}", -1);
            }
        }

        #endregion

        #region informes
        #endregion

        #region reactivaciones

        /// <summary>
        /// Obtiene la información de una operación para reactivación
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_solicitud"></param>
        /// <returns></returns>
        public ErrorDto<ReactivacionModel> Cr_TraspasoTes_Reactivacion_Buscar(int CodEmpresa, int id_solicitud)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var operacion = connection.QueryFirstOrDefault<ReactivacionModel>(
                    @"select R.id_solicitud, R.codigo, C.descripcion as descripcion_linea,
                             S.cedula, S.nombre, R.monto_girado, R.emitir
                      from reg_creditos R
                      inner join Socios S on R.cedula = S.cedula
                      inner join Catalogo C on R.codigo = C.codigo
                      where R.id_solicitud = @id_solicitud
                        and R.estado = 'A'",
                    new { id_solicitud });

                if (operacion is null)
                {
                    return DbHelper.CreateErrorResponse("La operación digitada no existe.", -1, new ReactivacionModel());
                }

                if (operacion.emitir != "CK" && operacion.emitir != "TE")
                {
                    operacion.permitido = false;
                    operacion.motivo = operacion.emitir switch
                    {
                        "RC" => "EL TIPO DE EMISION -Retiro de Efectivo en Cajas- NO PERMITE REACTIVACION",
                        "CP" => "EL TIPO DE EMISION -Pago a Proveedor- NO PERMITE REACTIVACION",
                        _ => "EL TIPO DE EMISION ACTUAL NO PERMITE REACTIVACION"
                    };
                    return DbHelper.CreateOkResponse(operacion);
                }

                var existenDocs = connection.QueryFirstOrDefault<int>(
                    @"select count(*) from Tes_Transacciones
                      where op = @id_solicitud and estado in('I','T','P')",
                    new { id_solicitud });

                if (existenDocs > 0)
                {
                    operacion.permitido = false;
                    operacion.motivo = "EXISTE UN DOCUMENTO O SOLICITUD DE EMISION EN TESORERIA";
                    return DbHelper.CreateOkResponse(operacion);
                }

                operacion.permitido = true;
                return DbHelper.CreateOkResponse(operacion);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al consultar la operación para reactivación: {ex.Message}", -1, new ReactivacionModel());
            }
        }

        /// <summary>
        /// Reactiva una operación (set tesoreria = null)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cr_TraspasoTes_Reactivacion_Ejecutar(int CodEmpresa, int id_solicitud, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var existe = connection.QueryFirstOrDefault<int>(
                    @"select count(*) from reg_creditos
                      where id_solicitud = @id_solicitud
                        and tesoreria is not null
                        and estado = 'A'",
                    new { id_solicitud });

                if (existe == 0)
                {
                    return DbHelper.ErrorResponse("La operación no puede ser reactivada.", -1);
                }

                connection.Execute(
                    "update reg_creditos set tesoreria = null where id_solicitud = @id_solicitud",
                    new { id_solicitud });

                RegistrarBitacora(CodEmpresa, usuario, $"ReActivacion Traslado Tes. Op: {id_solicitud}", "Aplica - WEB");

                _mtes.sbCrdOperacionTags(
                    CodEmpresa,
                    id_solicitud,
                    string.Empty,
                    "S04",
                    usuario,
                    string.Empty,
                    ">>> Re.Activación del Desembolso <<<");

                return DbHelper.OkResponse("Operación reactivada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al reactivar la operación: {ex.Message}", -1);
            }
        }

        #endregion

        #region cambio

        /// <summary>
        /// Obtiene los desembolsos de una operación para cambio de concepto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<CambioConceptoModel>> Cr_TraspasoTes_Cambio_Buscar(int CodEmpresa, int id_solicitud)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var lista = connection.Query<CambioConceptoModel>(
                    @"select D.id_desembolso, R.id_solicitud, R.codigo, D.monto, D.concepto
                      from reg_creditos R
                      inner join desembolsos D on R.id_solicitud = D.id_solicitud
                      where D.retener = 0
                        and R.tesoreria is null
                        and R.estadosol = 'F'
                        and R.id_solicitud = @id_solicitud",
                    new { id_solicitud }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al consultar desembolsos para cambio de concepto: {ex.Message}", -1, new List<CambioConceptoModel>());
            }
        }

        /// <summary>
        /// Actualiza el concepto de un desembolso
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Cambio_Ejecutar(int CodEmpresa, CambioConceptoRequest request, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                connection.Execute(
                    "update desembolsos set concepto = @concepto where id_desembolso = @id_desembolso",
                    new { request.id_desembolso, request.concepto });

                RegistrarBitacora(CodEmpresa, usuario, $"Cambio de Concepto de Desembolso de CRD ID: {request.id_desembolso}", "Cambio - WEB");

                return DbHelper.OkResponse("Cambio de concepto realizado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al cambiar el concepto del desembolso: {ex.Message}", -1);
            }
        }

        #endregion

        #region consultas

        /// <summary>
        /// Consulta la remesa donde se registró una operación
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_solicitud"></param>
        /// <returns></returns>
        public ErrorDto<ConsultaModel> Cr_TraspasoTes_Consulta_Operacion(int CodEmpresa, int id_solicitud)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var operacion = connection.QueryFirstOrDefault<(int id_solicitud, string codigo, string descripcion, string cedula, string nombre, decimal monto_girado)?>(
                    @"select R.id_solicitud, R.codigo, C.descripcion, S.cedula, S.nombre, R.monto_girado
                      from reg_creditos R
                      inner join Socios S on R.cedula = S.cedula
                      inner join Catalogo C on R.codigo = C.codigo
                      where R.id_solicitud = @id_solicitud
                        and R.estado in('A','C')",
                    new { id_solicitud });

                if (!operacion.HasValue)
                {
                    return DbHelper.CreateErrorResponse("La operación no existe.", -1, new ConsultaModel());
                }

                var remesa = connection.QueryFirstOrDefault<ConsultaModel>(
                    @"select R.id_solicitud, R.codigo, C.descripcion as descripcion_linea,
                             S.cedula, S.nombre, R.monto_girado,
                             Td.cod_remesa,
                             T.estado as estado_remesa, T.fecha as fecha_remesa,
                             T.usuario as usuario_remesa, T.monto as monto_remesa,
                             T.desembolsos as desembolsos_remesa
                      from reg_creditos R
                      inner join Socios S on R.cedula = S.cedula
                      inner join Catalogo C on R.codigo = C.codigo
                      inner join CRD_REMESAS_TES_DETALLE Td on R.id_solicitud = Td.id_solicitud
                      inner join CRD_REMESAS_TES T on Td.cod_remesa = T.cod_remesa
                      where R.id_solicitud = @id_solicitud",
                    new { id_solicitud });

                remesa ??= new ConsultaModel
                {
                    id_solicitud = operacion.Value.id_solicitud,
                    codigo = operacion.Value.codigo,
                    descripcion_linea = operacion.Value.descripcion,
                    cedula = operacion.Value.cedula,
                    nombre = operacion.Value.nombre,
                    monto_girado = operacion.Value.monto_girado
                };

                return DbHelper.CreateOkResponse(remesa);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error al consultar la remesa de la operación: {ex.Message}", -1, new ConsultaModel());
            }
        }

        #endregion

        #region aux.giro
        #endregion

        private string ObtenerOTokenGenerado(int codEmpresa, string usuario, SqlConnection connection)
        {
            const string queryToken = "select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";
            var token = connection.QueryFirstOrDefault<string?>(queryToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token.Trim();
            }

            _mtes.spTes_Token_New(codEmpresa, usuario);
            return connection.QueryFirstOrDefault<string?>(queryToken)?.Trim() ?? string.Empty;
        }

        private static string? NormalizarToken(string? token)
        {
            return string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}