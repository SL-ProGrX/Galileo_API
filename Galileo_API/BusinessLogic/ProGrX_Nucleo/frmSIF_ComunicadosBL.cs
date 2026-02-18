using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.BusinessLogic
{
    public class FrmSifComunicadosBL(IConfiguration config)
    {
        private readonly FrmSifComunicadosDB _db = new(config);


        public ErrorDto Comunicados_Insertar(int CodCliente, SifComunicadoDto comunicado)
        {
            return _db.Comunicados_Insertar(CodCliente, comunicado);
        }

        public ErrorDto<int> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _db.ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        public ErrorDto<SifComunicadoDto> Comunicado_Obtener(int CodEmpresa, int Cod_Comunicado)
        {
            return _db.Comunicado_Obtener(CodEmpresa, Cod_Comunicado);
        }

        public ErrorDto<List<SifComunicadoDto>> ComunicadosLista_Obtener(int CodEmpresa)
        {
            return _db.ComunicadosLista_Obtener(CodEmpresa);
        }

        public ErrorDto Comunicado_Actualizar(int CodEmpresa, SifComunicadoDto request)
        {
            return _db.Comunicado_Actualizar(CodEmpresa, request);
        }

    }
}