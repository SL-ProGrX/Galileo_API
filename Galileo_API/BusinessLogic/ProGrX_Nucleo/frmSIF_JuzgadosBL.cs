using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.BusinessLogic
{
    public class FrmSifJuzgadosBL(IConfiguration config)
    {
        private readonly FrmSifJuzgadosDB _db = new FrmSifJuzgadosDB(config);

        public ErrorDto SIF_Juzgados_Insertar(int CodCliente, JuzgadosDto juzgado)
        {
            return _db.SIF_Juzgados_Insertar(CodCliente, juzgado);
        }

        public ErrorDto<string> SIF_Juzgados_ConsultaAscDesc(int CodEmpresa, string consecutivo, string tipo)
        {
            return _db.SIF_Juzgados_ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        public ErrorDto<JuzgadosDto> SIF_Juzgados_Obtener(int CodEmpresa, string consecutivo)
        {
            return _db.SIF_Juzgados_Obtener(CodEmpresa, consecutivo);
        }

        public ErrorDto SIF_Juzgados_Actualizar(int CodEmpresa, JuzgadosDto request)
        {
            return _db.SIF_Juzgados_Actualizar(CodEmpresa, request);
        }

        public ErrorDto SIF_Juzgados_Eliminar(int codEmpresa, string consecutivo)
        {
            return _db.SIF_Juzgados_Eliminar(codEmpresa, consecutivo);
        }

        public ErrorDto<List<JuzgadosDto>> SIF_JuzgadosLista_Obtener(int codEmpresa)
        {
            return _db.SIF_JuzgadosLista_Obtener(codEmpresa);
        }
    }
}