using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using static Galileo.Models.ProGrX.Clientes.FrmAfLiquidacionWModels;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmAfLiquidacionwDb
    {

        /// <summary>
        /// Obtiene la lista de bancos disponibles para la liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionBancos>> AF_Liquidacion_Bancos_Obtener(int CodEmpresa, AfLiquidacionBancosFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de banco son requeridos.", -2, new List<AfLiquidacionBancos>());
            }

            return EjecutarStoredProcedureList<AfLiquidacionBancos>(
                CodEmpresa,
                SpBancos,
                new
                {
                    Usuario = NormalizarTexto(filtro.Usuario),
                    Divisa = NormalizarTexto(filtro.Divisa)
                });
        }

        /// <summary>
        /// Actualiza el estado de las renuncias para la liquidación. Este método se utiliza para marcar las renuncias que ya han sido procesadas y evitar que se incluyan en futuras liquidaciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionEmiteTDoc>> AF_Liquidacion_Emite_TDoc(int CodEmpresa, AfLiquidacionEmiteTDocFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de documento son requeridos.", -2, new List<AfLiquidacionEmiteTDoc>());
            }

            var result = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
                connection.Query<AfLiquidacionEmiteTDoc>(
                    SpRenunciaEmiteTDoc,
                    new
                    {
                        filtro.BancoId,
                        filtro.Mortalidad
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfLiquidacionEmiteTDoc>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al ejecutar procedimiento almacenado.",
                    result.Code.GetValueOrDefault(-1),
                    new List<AfLiquidacionEmiteTDoc>());
        }


        /// <summary>
        /// Obtiene la lista de tipos de acción disponibles para la liquidación, como renuncia, retiro, entre otros. Esta información se utiliza para categorizar las acciones que se pueden realizar durante el proceso de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_TipoAccion_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                SqlTipoAccion);
        }


        /// <summary>
        /// Obtiene el detalle de una causa específica para la liquidación, incluyendo información relevante como la descripción de la causa, el tipo de acción asociada y cualquier otro dato necesario para comprender el motivo de la causa. Esta información es crucial para tomar decisiones informadas durante el proceso de liquidación y para proporcionar claridad sobre las razones detrás de cada acción tomada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Causa"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionCausasDetalle?> AF_Liquidacion_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            return DbHelper.ExecuteSingleQuery<AfLiquidacionCausasDetalle>(
                _portalDb,
                CodEmpresa,
                SqlCausaDetalle,
                null,
                new { Causa });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionCuentaBancaria>> AF_Liquidacion_CuentasBancarias_Obtener(int CodEmpresa, AfLiquidacionCuentaBancariaFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de cuenta bancaria son requeridos.", -2, new List<AfLiquidacionCuentaBancaria>());
            }

            return EjecutarStoredProcedureList<AfLiquidacionCuentaBancaria>(
                CodEmpresa,
                SpCuentasBancarias,
                new
                {
                    Identificacion = NormalizarTexto(filtro.Identificacion),
                    filtro.BancoId,
                    filtro.DivisaCheck
                });
        }


        /// <summary>
        /// Actualiza el estado de las renuncias a 'V' (Vencida) para aquellas que tienen una fecha de vencimiento menor a la fecha actual. Este método se utiliza para mantener actualizada la información de las renuncias en el sistema y asegurar que las renuncias vencidas sean identificadas correctamente durante el proceso de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<short> AF_Liquidacion_Fondos(int CodEmpresa)
        {
            return DbHelper.ExecuteSingleQuery<short>(
                _portalDb,
                CodEmpresa,
                SqlFondos,
                0);
        }


        /// <summary>
        /// Activa el control de liquidación para la empresa especificada. Este método se utiliza para habilitar o deshabilitar el control de liquidación, lo que puede afectar la forma en que se procesan las renuncias y otras acciones relacionadas con la liquidación. Al activar el control, se pueden aplicar reglas adicionales o restricciones durante el proceso de liquidación para garantizar un manejo adecuado de las renuncias y otros eventos relacionados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_ActivarControl(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<short>(
                _portalDb,
                CodEmpresa,
                SqlActivarControl,
                0);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result == 1)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar control de liquidación.", result.Code.GetValueOrDefault(-1), false);
        }


        /// <summary>
        /// Obtiene la lista de renuncias sin liquidar o socios activos, dependiendo del estado del control de liquidación. Si el control de liquidación está activado, se obtienen las renuncias sin liquidar; de lo contrario, se obtienen los socios activos. Esta información es esencial para determinar qué renuncias o socios deben ser considerados durante el proceso de liquidación y para garantizar que se manejen adecuadamente según el estado del control.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="activar_control"></param>
        /// <returns></returns>
        public ErrorDto<List<object>> AF_Liquidacion_Renuncias_Obtener(int CodEmpresa, bool activar_control)
        {
            if (activar_control)
            {
                var renuncias = DbHelper.ExecuteListQuery<AfLiquidacionRenunciaSinLiquidar>(
                    _portalDb,
                    CodEmpresa,
                    SqlRenunciasSinLiquidar);

                return renuncias.Code == 0
                    ? DbHelper.CreateOkResponse(renuncias.Result?.Cast<object>().ToList() ?? new List<object>())
                    : DbHelper.CreateErrorResponse(renuncias.Description ?? "Error al obtener renuncias sin liquidar.", renuncias.Code.GetValueOrDefault(-1), new List<object>());
            }

            var socios = DbHelper.ExecuteListQuery<AfLiquidacionSocio>(
                _portalDb,
                CodEmpresa,
                SqlSociosActivos);

            return socios.Code == 0
                ? DbHelper.CreateOkResponse(socios.Result?.Cast<object>().ToList() ?? new List<object>())
                : DbHelper.CreateErrorResponse(socios.Description ?? "Error al obtener socios activos.", socios.Code.GetValueOrDefault(-1), new List<object>());
        }


        /// <summary>
        /// Obtiene la lista de socios que tienen renuncias sin liquidar o que están activos, dependiendo del estado del control de liquidación. Si el control de liquidación está activado, se obtienen los socios con renuncias sin liquidar; de lo contrario, se obtienen todos los socios. Esta información es crucial para identificar qué socios deben ser considerados durante el proceso de liquidación y para garantizar que se manejen adecuadamente según el estado del control.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="activar_control"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionSocio>> AF_Liquidacion_SociosRenuncia_Obtener(int CodEmpresa, bool activar_control)
        {
            return DbHelper.ExecuteListQuery<AfLiquidacionSocio>(
                _portalDb,
                CodEmpresa,
                activar_control ? SqlSociosRenunciaActiva : SqlSociosTodos);
        }


        /// <summary>
        /// Obtiene el detalle de un socio específico, incluyendo información relevante como su identificación, nombre, estado de renuncia, entre otros datos necesarios para comprender su situación en el proceso de liquidación. Esta información es esencial para tomar decisiones informadas durante el proceso de liquidación y para garantizar que se manejen adecuadamente las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionSocioDetalle?> AF_Liquidacion_SocioDetalle_Obtener(int CodEmpresa, string Cedula)
        {
            return DbHelper.ExecuteSingleQuery<AfLiquidacionSocioDetalle>(
                _portalDb,
                CodEmpresa,
                SqlSocioDetalle,
                null,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene la lista de causas de renuncia disponibles para la liquidación, filtradas por el tipo de aplicación (I para individual o P para planilla). Esta información es esencial para categorizar las causas de renuncia y para garantizar que se manejen adecuadamente durante el proceso de liquidación, permitiendo una mejor comprensión de los motivos detrás de cada renuncia y facilitando la toma de decisiones informadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_CausasRenuncia_Obtener(int CodEmpresa, string tipo)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                SqlCausasRenuncia,
                new { TipoApl = NormalizarTipoAplicacion(tipo) });
        }


        /// <summary>
        /// Obtiene la acción asociada a una causa de renuncia específica, incluyendo información relevante como el tipo de acción, la descripción y cualquier otro dato necesario para comprender la relación entre la causa de renuncia y la acción que se debe tomar durante el proceso de liquidación. Esta información es crucial para garantizar que se manejen adecuadamente las acciones relacionadas con cada causa de renuncia y para facilitar la toma de decisiones informadas durante el proceso de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="IdCausa"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionCausaAccion?> AF_Liquidacion_Causas_Accion(int CodEmpresa, int IdCausa)
        {
            return DbHelper.ExecuteSingleQuery<AfLiquidacionCausaAccion>(
                _portalDb,
                CodEmpresa,
                SqlCausaAccion,
                null,
                new { IdCausa });
        }


        /// <summary>
        /// Verifica si un socio existe en el sistema, utilizando su cédula como identificador. Este método es fundamental para validar la existencia de un socio antes de realizar cualquier acción relacionada con la liquidación, asegurando que se manejen adecuadamente los casos en los que un socio no exista y evitando errores durante el proceso de liquidación. La respuesta incluye un indicador de existencia que puede ser utilizado para tomar decisiones informadas durante el proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionSocioExiste?> AF_Liquidacion_SocioExiste(int CodEmpresa, string Cedula)
        {
            return DbHelper.ExecuteSingleQuery<AfLiquidacionSocioExiste>(
                _portalDb,
                CodEmpresa,
                SqlSocioExiste,
                null,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene el código de renuncia más reciente para un socio específico, siempre y cuando exista una renuncia sin liquidar o en estado pendiente o vencida. Este método es esencial para identificar la renuncia más relevante para un socio durante el proceso de liquidación, permitiendo una mejor gestión de las renuncias y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionConsultaPatrimonio>> AF_Liquidacion_Consulta_Patrimonio(int CodEmpresa, string Cedula)
        {
            return EjecutarStoredProcedureList<AfLiquidacionConsultaPatrimonio>(
                CodEmpresa,
                SpConsultaPatrimonio,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene la información de renta global para un socio específico, utilizando su cédula como identificador. Esta información incluye detalles relevantes sobre la renta global del socio, que pueden ser utilizados durante el proceso de liquidación para tomar decisiones informadas sobre las acciones a tomar en relación con cada socio. La renta global es un factor importante a considerar durante la liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionRentaGlobal?> AF_Liquidacion_Renta_Global(int CodEmpresa, AfLiquidacionRentaGlobalFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse<AfLiquidacionRentaGlobal?>("Los filtros de renta global son requeridos.", -2, null);
            }

            return EjecutarStoredProcedureSingle<AfLiquidacionRentaGlobal>(
                CodEmpresa,
                SpRentaGlobal,
                new
                {
                    Cedula = NormalizarTexto(filtro.Cedula),
                    filtro.Corte,
                    filtro.MntRetiro,
                    Plan = NormalizarTexto(filtro.Plan)
                });
        }


        /// <summary>
        /// Obtiene la lista de planes asociados a un socio específico, utilizando su cédula como identificador. Esta información es esencial para comprender la relación entre el socio y los planes que tiene asociados, lo que puede afectar el proceso de liquidación y las acciones relacionadas con cada socio. La lista de planes proporciona detalles sobre los productos o servicios que el socio tiene contratados, lo que puede ser relevante para determinar el monto a liquidar y las acciones a tomar durante el proceso de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionListaPlanes>> AF_Liquidacion_ListaPlanes(int CodEmpresa, AfLiquidacionListaPlanesFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de planes son requeridos.", -2, new List<AfLiquidacionListaPlanes>());
            }

            return EjecutarStoredProcedureList<AfLiquidacionListaPlanes>(
                CodEmpresa,
                SpListaPlanes,
                new
                {
                    Cedula = NormalizarTexto(filtro.Cedula),
                    filtro.TipoLiq
                });
        }


        /// <summary>
        /// Obtiene la lista de créditos asociados a un socio específico, utilizando su cédula como identificador. Esta información es crucial para comprender la situación financiera del socio y para tomar decisiones informadas durante el proceso de liquidación, ya que los créditos pueden afectar el monto a liquidar y las acciones relacionadas con cada socio. La lista de créditos proporciona detalles sobre los préstamos o financiamientos que el socio tiene vigentes, lo que puede ser relevante para determinar el impacto de estos créditos en la liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionCreditosPersona>> AF_Liquidacion_CreditosPersona(int CodEmpresa, AfLiquidacionCreditosPersonaFiltro filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de créditos son requeridos.", -2, new List<AfLiquidacionCreditosPersona>());
            }

            return EjecutarStoredProcedureList<AfLiquidacionCreditosPersona>(
                CodEmpresa,
                SpCreditosPersona,
                new
                {
                    Cedula = NormalizarTexto(filtro.Cedula),
                    filtro.Abono
                });
        }


        /// <summary>
        /// Obtiene el código de renuncia más reciente para un socio específico, siempre y cuando exista una renuncia sin liquidar o en estado pendiente o vencida. Este método es esencial para identificar la renuncia más relevante para un socio durante el proceso de liquidación, permitiendo una mejor gestión de las renuncias y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionCodRenuncia?> AF_Liquidacion_CodRenuncia_Obtener(int CodEmpresa, string Cedula)
        {
            return DbHelper.ExecuteSingleQuery<AfLiquidacionCodRenuncia>(
                _portalDb,
                CodEmpresa,
                SqlCodRenuncia,
                null,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene la información básica de un socio específico, utilizando su cédula como identificador. Esta información incluye detalles relevantes como el número de cuenta, el identificador del promotor y el identificador de la boleta de afiliación, que pueden ser utilizados durante el proceso de liquidación para tomar decisiones informadas sobre las acciones a tomar en relación con cada socio. La información básica del socio es fundamental para comprender su situación en el proceso de liquidación y para garantizar que se manejen adecuadamente las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<AfSocioDatosBasicos?> AF_Liquidacion_SocioDatosBasicos_Obtener(int CodEmpresa, string Cedula)
        {
            return DbHelper.ExecuteSingleQuery<AfSocioDatosBasicos>(
                _portalDb,
                CodEmpresa,
                SqlSocioDatosBasicos,
                null,
                new { Cedula = NormalizarTexto(Cedula) });
        }


        /// <summary>
        /// Obtiene la lista de morosidad asociada a una solicitud específica, utilizando el identificador de la solicitud como filtro. Esta información es crucial para comprender la situación financiera del socio en relación con la solicitud y para tomar decisiones informadas durante el proceso de liquidación, ya que la morosidad puede afectar el monto a liquidar y las acciones relacionadas con cada socio. La lista de morosidad proporciona detalles sobre los pagos atrasados o incumplidos asociados a la solicitud, lo que puede ser relevante para determinar el impacto de esta morosidad en la liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<AfMorosidadConsultaModel>> AF_Morosidad_ConsultarPorSolicitud(int CodEmpresa, int IdSolicitud)
        {
            return DbHelper.ExecuteListQuery<AfMorosidadConsultaModel>(
                _portalDb,
                CodEmpresa,
                SqlMorosidadPorSolicitud,
                new { IdSolicitud });
        }
    }
}