using Galileo.DataBaseTier.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.BusinessLogic.ProGrX.Credito
{
    public class FrmCRTraspasoTesoreriaBL
    {
        private readonly FrmCRTraspasoTesoreriaDB _db;

        public FrmCRTraspasoTesoreriaBL(IConfiguration config)
        {
            _db = new FrmCRTraspasoTesoreriaDB(config);
        }

        #region remesas

        /// <summary>
        /// Obtiene las ultimas 50 remesas registradas
        /// </summary>
        public ErrorDto<List<RemesaModel>> Cr_TraspasoTes_Remesas_Listar(int CodEmpresa)
        {
            return _db.Cr_TraspasoTes_Remesas_Listar(CodEmpresa);
        }

        /// <summary>
        /// Obtiene una remesa por su codigo
        /// </summary>
        public ErrorDto<RemesaModel> Cr_TraspasoTes_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.Cr_TraspasoTes_Remesa_Obtener(CodEmpresa, cod_remesa);
        }

        /// <summary>
        /// Crea una nueva remesa
        /// </summary>
        public ErrorDto<RemesaModel> Cr_TraspasoTes_Remesa_Crear(int CodEmpresa, RemesaRequest request, string usuario)
        {
            return _db.Cr_TraspasoTes_Remesa_Crear(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Actualiza una remesa existente
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Remesa_Modificar(int CodEmpresa, RemesaRequest request, string usuario)
        {
            return _db.Cr_TraspasoTes_Remesa_Modificar(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Elimina una remesa
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Remesa_Eliminar(int CodEmpresa, int cod_remesa, string usuario)
        {
            return _db.Cr_TraspasoTes_Remesa_Eliminar(CodEmpresa, cod_remesa, usuario);
        }

        #endregion

        #region cargar

        /// <summary>
        /// Obtiene las remesas en estado Abierta para el combo de carga
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_RemesasAbiertas_Obtener(int CodEmpresa)
        {
            return _db.Cr_TraspasoTes_RemesasAbiertas_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Obtiene las operaciones disponibles para cargar en una remesa
        /// </summary>
        public ErrorDto<List<CargaOperacionModel>> Cr_TraspasoTes_Carga_Buscar(int CodEmpresa, int cod_remesa)
        {
            return _db.Cr_TraspasoTes_Carga_Buscar(CodEmpresa, cod_remesa);
        }

        /// <summary>
        /// Carga operaciones seleccionadas a una remesa
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Carga_Ejecutar(int CodEmpresa, CargaRequest request, string usuario)
        {
            return _db.Cr_TraspasoTes_Carga_Ejecutar(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Cierra una remesa
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Remesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            return _db.Cr_TraspasoTes_Remesa_Cerrar(CodEmpresa, cod_remesa, usuario);
        }

        #endregion

        #region trasladar

        /// <summary>
        /// Obtiene las remesas en estado Cerradas para el traspaso a tesoreria
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            return _db.Cr_TraspasoTes_Remesas_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Obtiene los tokens disponibles para el traslado
        /// </summary>
        public ErrorDto<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _db.Cr_TraspasoTesToken_Obtener(CodEmpresa, usuario);
        }

        /// <summary>
        /// Genera un nuevo token para el traslado
        /// </summary>
        public ErrorDto Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _db.Cr_TraspasoTesToken_Nuevo(CodEmpresa, usuario);
        }

        /// <summary>
        /// Busca las solicitudes asociadas a una remesa para ser trasladadas
        /// </summary>
        public ErrorDto<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            return _db.Cr_TraspasoTesTraslado_Buscar(CodEmpresa, cod_remesa);
        }

        /// <summary>
        /// Ejecuta el traspaso de solicitudes a tesoreria
        /// </summary>
        public ErrorDto CrTraspasoTes_Traslado_Generar(int CodEmpresa, int cod_remesa, string usuario, string? token)
        {
            return _db.CrTraspasoTes_Traslado_Generar(CodEmpresa, cod_remesa, usuario, token);
        }

        #endregion

        #region informes
        #endregion

        #region reactivaciones

        /// <summary>
        /// Obtiene la información de una operación para reactivación
        /// </summary>
        public ErrorDto<ReactivacionModel> Cr_TraspasoTes_Reactivacion_Buscar(int CodEmpresa, int id_solicitud)
        {
            return _db.Cr_TraspasoTes_Reactivacion_Buscar(CodEmpresa, id_solicitud);
        }

        /// <summary>
        /// Reactiva una operación
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Reactivacion_Ejecutar(int CodEmpresa, int id_solicitud, string usuario)
        {
            return _db.Cr_TraspasoTes_Reactivacion_Ejecutar(CodEmpresa, id_solicitud, usuario);
        }

        #endregion

        #region cambio

        /// <summary>
        /// Obtiene los desembolsos de una operación para cambio de concepto
        /// </summary>
        public ErrorDto<List<CambioConceptoModel>> Cr_TraspasoTes_Cambio_Buscar(int CodEmpresa, int id_solicitud)
        {
            return _db.Cr_TraspasoTes_Cambio_Buscar(CodEmpresa, id_solicitud);
        }

        /// <summary>
        /// Actualiza el concepto de un desembolso
        /// </summary>
        public ErrorDto Cr_TraspasoTes_Cambio_Ejecutar(int CodEmpresa, CambioConceptoRequest request, string usuario)
        {
            return _db.Cr_TraspasoTes_Cambio_Ejecutar(CodEmpresa, request, usuario);
        }

        #endregion

        #region consultas

        /// <summary>
        /// Consulta la remesa donde se registró una operación
        /// </summary>
        public ErrorDto<ConsultaModel> Cr_TraspasoTes_Consulta_Operacion(int CodEmpresa, int id_solicitud)
        {
            return _db.Cr_TraspasoTes_Consulta_Operacion(CodEmpresa, id_solicitud);
        }

        #endregion

        #region aux.giro
        #endregion
    }
}
