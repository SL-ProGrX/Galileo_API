using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoGestorExternoModels;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoGestorExternoBL
    {
        private readonly FrmCoGestorExternoDB _db;

        public FrmCoGestorExternoBL(IConfiguration config)
        {
            _db = new FrmCoGestorExternoDB(config);
        }

        public ErrorDto<List<CrdGestorExternoListaItemModel>> Crd_GestorExterno_Listado_Obtener(int CodEmpresa, CrdGestorExternoFiltroRequest request)
                => _db.Crd_GestorExterno_Listado_Obtener(CodEmpresa, request);
        public ErrorDto<string> Crd_GestorExterno_Registrar(int CodEmpresa, CrdGestorExternoRegistrarRequest request)
                        => _db.Crd_GestorExterno_Registrar(CodEmpresa, request);
        public ErrorDto<bool> Crd_GestorExterno_Reversar(int CodEmpresa, CrdGestorExternoReversaRequest request)
                => _db.Crd_GestorExterno_Reversar(CodEmpresa, request);
        public ErrorDto<CrdGestorExternoCargaMasivaResponse> Crd_GestorExterno_CargaMasiva_Procesar(int CodEmpresa, CrdGestorExternoCargaMasivaRequest request)
                 => _db.Crd_GestorExterno_CargaMasiva_Procesar(CodEmpresa, request);
        public ErrorDto<List<CrdGestorExternoOperacionModel>> Crd_GestorExterno_Operacion_Buscar(int CodEmpresa)
                 => _db.Crd_GestorExterno_Operacion_Buscar(CodEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_GestorExterno_Gestores_Obtener(int codEmpresa)
                 => _db.Crd_GestorExterno_Gestores_Obtener(codEmpresa);


    }
}
