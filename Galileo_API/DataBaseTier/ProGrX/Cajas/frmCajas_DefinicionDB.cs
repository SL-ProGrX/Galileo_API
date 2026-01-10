using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasDefinicionDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private const int vModulo = 36;
        private const string error = "Error";

        public FrmCajasDefinicionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => DBBitacora.Bitacora(data);

        /// <summary>
        /// Obtiene las oficinas activas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de oficinas activas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerOficinasActivas(int codEmpresa)
        {
            var query = @"SELECT rtrim(cod_oficina) AS item, rtrim(Descripcion) AS descripcion
                          FROM sif_oficinas
                          WHERE estado = 1
                          ORDER BY cod_oficina";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de cajas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de cajas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasDefinicion_Cajas_Obtener(int codEmpresa)
        {
            var query = @"SELECT rtrim(cod_caja) AS item, rtrim(descripcion) AS descripcion
                          FROM cajas_definicion
                          ORDER BY cod_caja";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el detalle de una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="gEnlace">Enlace de contabilidad.</param>
        /// <returns>Detalle de la caja.</returns>
        public ErrorDto<CajasDefinicionDetalleModel?> CajasDefinicion_CajaDetalle_Obtener(int codEmpresa, string codCaja, string gEnlace)
        {
            var query = @"
                select C.*,
                       rtrim(O.Cod_Oficina) as Cod_Oficina,
                       rtrim(O.Descripcion) as OficinaDesc,
                       isnull(Cta.Descripcion,'') as CuentaDesc
                from cajas_definicion C
                inner join Sif_Oficinas O
                        on C.cod_Oficina = O.cod_Oficina
                left join CntX_Cuentas Cta
                       on C.Cod_Cuenta_Dev = Cta.Cod_Cuenta
                      and Cta.Cod_Contabilidad = @gEnlace
                where C.cod_caja = @codCaja";
            var parameters = new { codCaja, gEnlace };
            return DbHelper.ExecuteSingleQuery<CajasDefinicionDetalleModel>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Obtiene la política de divisas de una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="gEnlace">Enlace de contabilidad.</param>
        /// <returns>Lista de políticas de divisas.</returns>
        public ErrorDto<List<CajasDivisaPoliticaModel>> CajasDefinicion_DivisasPolitica_Obtener(int codEmpresa, string codCaja, string gEnlace)
        {
            var query = @"
                Select D.Cod_Divisa,
                       D.descripcion,
                       isnull(P.efectivo_maximo,0) as Efec_Max,
                       isnull(P.Efectivo_minimo,0) as Efec_Min,
                       isnull(P.Documentos_Maximo, 0) As Doc_Max,
                       isnull(P.Documentos_minimo,0) as Doc_Min
                from cntx_divisas D
                left join cajas_politicas_saldos P
                       on D.Cod_divisa = P.cod_divisa
                      and P.cod_caja = @codCaja
                where D.cod_Contabilidad = @gEnlace";
            var parameters = new { codCaja, gEnlace };
            return DbHelper.ExecuteListQuery<CajasDivisaPoliticaModel>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene la lista de recaudadores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de recaudadores.</returns>
        public ErrorDto<List<CajasRecaudadorModel>> CajasDefinicion_Recaudadores_Obtener(int codEmpresa)
        {
            var query = @"select cod_recaudador, descripcion
                          from cajas_recaudador
                          order by cod_recaudador";
            return DbHelper.ExecuteListQuery<CajasRecaudadorModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los servicios asignados a una caja y recaudador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="codRecaudador">Código del recaudador.</param>
        /// <returns>Lista de servicios asignados.</returns>
        public ErrorDto<List<CajasServicioAsignadoModel>> CajasDefinicion_ServiciosAsignados_Obtener(int codEmpresa, string codCaja, string codRecaudador)
        {
            var query = @"
                select C.cod_servicio,
                       C.descripcion,
                       X.cod_servicio as Asignado
                from cajas_Servicios C
                left join cajas_servicios_asignados X
                       on C.cod_servicio = X.cod_servicio
                      and X.cod_recaudador = @codRecaudador
                      and X.Cod_caja = @codCaja
                where C.cod_recaudador = @codRecaudador
                order by X.cod_servicio desc, C.cod_servicio";
            var parameters = new { codCaja, codRecaudador };
            return DbHelper.ExecuteListQuery<CajasServicioAsignadoModel>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Inserta un servicio asignado a una caja y recaudador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del servicio asignado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_ServicioAsignar_Insertar(int codEmpresa, CajasServicioAsignarParams param)
        {
            var query = @"
                insert into cajas_servicios_asignados
                (cod_recaudador, cod_caja, cod_servicio, registro_usuario, registro_fecha)
                values
                (@Cod_Recaudador, @Cod_Caja, @Cod_Servicio, @Usuario, dbo.MyGetdate())";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un servicio asignado de una caja y recaudador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del servicio asignado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_ServicioAsignar_Eliminar(int codEmpresa, CajasServicioAsignarParams param)
        {
            var query = @"
                delete from cajas_servicios_asignados
                where cod_caja = @Cod_Caja
                  and cod_servicio = @Cod_Servicio
                  and cod_recaudador = @Cod_Recaudador";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene los auxiliares de catálogo asignados a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>Lista de auxiliares de catálogo.</returns>
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresCatalogo_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            string query = @"
                select A.CODIGO AS Codigo,
                       A.descripcion,
                       C.COD_AUXILIAR as Asignado
                from CATALOGO A
                left join CAJAS_AUXILIARES_ASG C
                       on A.CODIGO = C.COD_AUXILIAR
                      AND C.TIPO = 'CRD'
                      and C.cod_caja = @CodCaja";
            object parameters;
            if (!string.IsNullOrWhiteSpace(param.AuxFiltro))
            {
                query += " Where A.Descripcion like @AuxFiltro";
                parameters = new { param.CodCaja, AuxFiltro = "%" + param.AuxFiltro + "%" };
            }
            else
            {
                parameters = new { param.CodCaja };
            }
            query += " order by C.COD_AUXILIAR desc, A.CODIGO";
            return DbHelper.ExecuteListQuery<CajasAuxiliarAsignadoModel>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene los auxiliares de fondos asignados a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>Lista de auxiliares de fondos.</returns>
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresFondos_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            string query = @"
                select A.COD_PLAN AS Codigo,
                       A.descripcion,
                       C.COD_AUXILIAR as Asignado
                from FND_PLANES A
                left join CAJAS_AUXILIARES_ASG C
                       on A.COD_PLAN = C.COD_AUXILIAR
                      AND C.TIPO = 'FND'
                      and C.cod_caja = @CodCaja";
            object parameters;
            if (!string.IsNullOrWhiteSpace(param.AuxFiltro))
            {
                query += " Where A.Descripcion like @AuxFiltro";
                parameters = new { param.CodCaja, AuxFiltro = "%" + param.AuxFiltro + "%" };
            }
            else
            {
                parameters = new { param.CodCaja };
            }
            query += " order by C.COD_AUXILIAR desc, A.COD_PLAN";
            return DbHelper.ExecuteListQuery<CajasAuxiliarAsignadoModel>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene los auxiliares de CxC asignados a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>Lista de auxiliares de CxC.</returns>
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresCxc_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            string query = @"
                select A.COD_CONCEPTO AS Codigo,
                       A.descripcion,
                       C.COD_AUXILIAR as Asignado
                from CXC_CONCEPTOS A
                left join CAJAS_AUXILIARES_ASG C
                       on A.COD_CONCEPTO = C.COD_AUXILIAR
                      AND C.TIPO = 'CXC'
                      and C.cod_caja = @CodCaja";
            object parameters;
            if (!string.IsNullOrWhiteSpace(param.AuxFiltro))
            {
                query += " Where A.Descripcion like @AuxFiltro";
                parameters = new { param.CodCaja, AuxFiltro = "%" + param.AuxFiltro + "%" };
            }
            else
            {
                parameters = new { param.CodCaja };
            }
            query += " order by C.COD_AUXILIAR desc, A.COD_CONCEPTO";
            return DbHelper.ExecuteListQuery<CajasAuxiliarAsignadoModel>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene los auxiliares FFP asignados a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>Lista de auxiliares FFP.</returns>
        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresFfp_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            string query = @"
                select A.COD_PLAN AS Codigo,
                       A.descripcion,
                       C.COD_AUXILIAR as Asignado
                from FND_PLANES A
                left join CAJAS_AUXILIARES_ASG C
                       on A.COD_PLAN = C.COD_AUXILIAR
                      AND C.TIPO = 'FFP'
                      and C.cod_caja = @CodCaja
                Where A.PERMITE_RETIROS_CAJAS = 1
                  and A.TIPO_CDP = 0";
            object parameters;
            if (!string.IsNullOrWhiteSpace(param.AuxFiltro))
            {
                query += " and A.Descripcion like @AuxFiltro";
                parameters = new { param.CodCaja, AuxFiltro = "%" + param.AuxFiltro + "%" };
            }
            else
            {
                parameters = new { param.CodCaja };
            }
            query += " order by C.COD_AUXILIAR desc, A.COD_PLAN";
            return DbHelper.ExecuteListQuery<CajasAuxiliarAsignadoModel>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Inserta un auxiliar asignado a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del auxiliar asignado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_AuxiliarAsignar_Insertar(int codEmpresa, string usuario, CajasAuxiliarAsignarParams param)
        {
            var query = @"
                insert into CAJAS_AUXILIARES_ASG
                (tipo, cod_auxiliar, cod_caja, registro_fecha, registro_usuario)
                values
                (@Tipo, @CodAuxiliar, @CodCaja, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, $"REGISTRA CONCEPTO: {param.CodAuxiliar} en Caja: {param.CodCaja}", "REGISTRA - WEB");
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un auxiliar asignado de una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del auxiliar asignado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_AuxiliarAsignar_Eliminar(int codEmpresa, string usuario, CajasAuxiliarAsignarParams param)
        {
            var query = @"
                delete from CAJAS_AUXILIARES_ASG
                where tipo = @Tipo
                  and cod_auxiliar = @CodAuxiliar
                  and cod_caja = @CodCaja";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, $"ELIMINA CONCEPTO: {param.CodAuxiliar} de Caja: {param.CodCaja}", "ELIMINA - WEB");
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene las formas de pago asignadas a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <returns>Lista de formas de pago.</returns>
        public ErrorDto<List<CajasFormaPagoAsignadoModel>> CajasDefinicion_FormasPago_Obtener(int codEmpresa, string codCaja)
        {
            var query = @"
                select F.COD_FORMA_PAGO,
                       F.DESCRIPCION,
                       C.cod_caja as Asignado
                from SIF_FORMAS_PAGO F
                left join CAJAS_FORMAS_PAGO C
                       on F.COD_FORMA_PAGO = C.COD_FORMA_PAGO
                      AND F.Activa = 1
                      and C.cod_caja = @CodCaja
                order by C.cod_Caja desc, F.cod_Forma_Pago";
            return DbHelper.ExecuteListQuery<CajasFormaPagoAsignadoModel>(_portalDb, codEmpresa, query, new { CodCaja = codCaja });
        }

        /// <summary>
        /// Inserta una forma de pago asignada a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la forma de pago.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_FormaPagoAsignar_Insertar(int codEmpresa, CajasFormaPagoAsignarParams param)
        {
            var query = @"
                insert into CAJAS_FORMAS_PAGO
                (cod_forma_pago, cod_caja, registro_fecha, registro_usuario)
                values
                (@Cod_Forma_Pago, @Cod_Caja, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina una forma de pago asignada de una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la forma de pago.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_FormaPagoAsignar_Eliminar(int codEmpresa, CajasFormaPagoAsignarParams param)
        {
            var query = @"
                delete from CAJAS_FORMAS_PAGO
                where cod_forma_pago = @Cod_Forma_Pago
                  and cod_caja = @Cod_Caja";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene los documentos asignados a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <returns>Lista de documentos asignados.</returns>
        public ErrorDto<List<CajasDocumentoAsignadoModel>> CajasDefinicion_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            var query = @"
                select D.TIPO_DOCUMENTO,
                       D.DESCRIPCION,
                       C.cod_caja as Asignado
                from SIF_DOCUMENTOS D
                left join CAJAS_DOCUMENTOS C
                       on D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                      AND D.Activo = 1
                      and C.cod_caja = @CodCaja
                order by C.cod_Caja desc, D.TIPO_DOCUMENTO";
            return DbHelper.ExecuteListQuery<CajasDocumentoAsignadoModel>(_portalDb, codEmpresa, query, new { CodCaja = codCaja });
        }

        /// <summary>
        /// Inserta un documento asignado a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del documento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_DocumentoAsignar_Insertar(int codEmpresa, CajasDocumentoAsignarParams param)
        {
            var query = @"
                insert into CAJAS_DOCUMENTOS
                (TIPO_DOCUMENTO, cod_caja, registro_fecha, registro_usuario)
                values
                (@Tipo_Documento, @Cod_Caja, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un documento asignado de una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del documento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_DocumentoAsignar_Eliminar(int codEmpresa, CajasDocumentoAsignarParams param)
        {
            var query = @"
                delete from CAJAS_DOCUMENTOS
                where TIPO_DOCUMENTO = @Tipo_Documento
                  and cod_caja = @Cod_Caja";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Obtiene el historial de usuarios de una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <returns>Lista de historial de usuarios.</returns>
        public ErrorDto<List<CajasUsuarioHistorialModel>> CajasDefinicion_UsuariosHistorial_Obtener(int codEmpresa, string codCaja)
        {
            var query = @"
                select rtrim(usuario) as Usuario,
                       registro_fecha,
                       rtrim(registro_usuario) as Registro_Usuario,
                       salida_fecha,
                       salida_usuario
                from cajas_usuarios_h
                where cod_caja = @CodCaja
                order by usuario";
            return DbHelper.ExecuteListQuery<CajasUsuarioHistorialModel>(_portalDb, codEmpresa, query, new { CodCaja = codCaja });
        }

        /// <summary>
        /// Inserta una nueva caja en cajas_definicion.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la caja.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_Caja_Insertar(int codEmpresa, CajasDefinicionInsertParams param)
        {
            var query = @"
                insert into cajas_definicion(
                  cod_caja,descripcion,notas,activa,apertura_fecha,apertura_compartida,
                  cierre_periocidad,cierre_tipo,periocidad_contrasena,oficina_utiliza_usuario,cod_oficina,
                  cod_cuenta_dev,PERMITE_MOV_CBRJUD, Limita_Consulta, Limita_Creditos, Limita_Fondos, Limita_CxC,
                  Limita_Patrimonio, PERMITE_RC, PERMITE_TRASLADOS_EF , ROL_BOVEDA, UTILIZA_CTA_CAJA_EF, LIMITA_FONDOS_FP,
                  REGISTRO_FECHA,REGISTRO_USUARIO
                )
                values(
                  @Cod_Caja,@Descripcion,@Notas,@Activa,@Apertura_Fecha,@Apertura_Compartida,
                  @Cierre_Periocidad,@Cierre_Tipo,@Periocidad_Contrasena,@Oficina_Utiliza_Usuario,@Cod_Oficina,
                  @Cod_Cuenta_Dev,@Permite_Mov_Cbrjud,@Limita_Consulta,@Limita_Creditos,@Limita_Fondos,@Limita_Cxc,
                  @Limita_Patrimonio,@Permite_Rc,@Permite_Traslados_Ef,@Rol_Boveda,@Utiliza_Cta_Caja_Ef,@Limita_Fondos_Fp,
                  dbo.MyGetdate(),@Registro_Usuario
                )";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, param.Registro_Usuario, $"Definición Cajas: {param.Cod_Caja}", "Registra - WEB");
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Ejecuta el procedimiento de copia de caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la copia.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_Caja_Copiar(int codEmpresa, CajasDefinicionCopiaParams param)
        {
            var query = "exec spCaja_Copia @CajaOrigen, @CajaDestino, @Usuario, @CajaNombre";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, param.Usuario, $"Copia Caja: {param.CajaOrigen} a {param.CajaDestino}", "Aplica - WEB");
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina una caja de cajas_definicion.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> CajasDefinicion_Caja_Eliminar(int codEmpresa, string codCaja, string usuario)
        {
            var query = @"
                delete from cajas_definicion
                where cod_caja = @CodCaja";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { CodCaja = codCaja });

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, $"Caja : {codCaja}", "Elimina - WEB");
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
