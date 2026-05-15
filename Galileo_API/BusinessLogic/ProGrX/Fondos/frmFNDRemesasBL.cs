using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndRemesasBl
    {
        private readonly FrmFndRemesasDb _db;

        public FrmFndRemesasBl(IConfiguration config)
        {
            _db = new FrmFndRemesasDb(config);
        }

        public ErrorDto<FndRemesasData> FND_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            return _db.FND_Remesa_Obtener(CodEmpresa, Remesa);
        }

        public ErrorDto<List<FndRemesasData>> FND_Remesas_Lista_Obtener(int CodEmpresa, int TabIndex, int Lineas)
        {
            return _db.FND_Remesas_Lista_Obtener(CodEmpresa, TabIndex, Lineas);
        }

        public ErrorDto FND_Remesas_Guardar(int CodEmpresa, FndRemesasData RemesaData)
        {
            return _db.FND_Remesas_Guardar(CodEmpresa, RemesaData);
        }

        public ErrorDto FND_Remesas_Eliminar(int CodEmpresa, int Remesa, string Usuario)
        {
            return _db.FND_Remesas_Eliminar(CodEmpresa, Remesa, Usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Remesas_Bancos_Obtener(int CodEmpresa, int Remesa)
        {
            return _db.FND_Remesas_Bancos_Obtener(CodEmpresa, Remesa);
        }

        public ErrorDto<List<FndRemesasCargaData>> FND_Remesa_Carga_Obtener(int CodEmpresa, int Remesa, int Banco)
        {
            return _db.FND_Remesa_Carga_Obtener(CodEmpresa, Remesa, Banco);
        }

        public ErrorDto FND_Remesas_Carga_Procesar(int CodEmpresa, int Remesa, string Usuario, List<int> ConsecSeleccionados)
        {
            return _db.FND_Remesas_Carga_Procesar(CodEmpresa, Remesa, Usuario, ConsecSeleccionados);
        }

        public ErrorDto FND_Remesas_Carga_Cerrar(int CodEmpresa, int Remesa, string Usuario)
        {
            return _db.FND_Remesas_Carga_Cerrar(CodEmpresa, Remesa, Usuario);
        }

        public ErrorDto<string> FND_Remesas_ConsultaRetiro_Obtener(int CodEmpresa, int Consec)
        {
            return _db.FND_Remesas_ConsultaRetiro_Obtener(CodEmpresa, Consec);
        }
    }
}