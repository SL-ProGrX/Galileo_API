using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosAdicionRetiroBL
    {
        private readonly FrmActivosAdicionRetiroDb _db;

        public FrmActivosAdicionRetiroBL(IConfiguration config)
        {
            _db = new FrmActivosAdicionRetiroDb(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Justificaciones_Obtener(int CodEmpresa, string tipo)
        {
            return _db.Activos_AdicionRetiro_Justificaciones_Obtener(CodEmpresa, tipo);
        }
        public ErrorDto<ActivosRetiroAdicionData> Activos_AdicionRetiro_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            return _db.Activos_AdicionRetiro_Consultar(CodEmpresa, Id_AddRet, placa);
        }
        public ErrorDto<string> Activos_AdicionRetiro_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            return _db.Activos_AdicionRetiro_Validar(CodEmpresa,  placa, fecha);
        }
        public ErrorDto<int> Activos_AdicionRetiro_Meses_Consulta(int CodEmpresa, string placa, string tipo, DateTime fecha)
        {
            return _db.Activos_AdicionRetiro_Meses_Consulta(CodEmpresa, placa, tipo,fecha);
        }
        public ErrorDto<ActivosPrincipalData> Activos_AdicionRetiro_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_AdicionRetiro_DatosActivo_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_AdicionRetiro_Proveedores_Obtener(int CodEmpresa)
        {
            return _db.Activos_AdicionRetiro_Proveedores_Obtener(CodEmpresa);
        }
        public ErrorDto<List<ActivosData>> Activos_AdicionRetiro_Activos_Obtener(int CodEmpresa)
        {
            return _db.Activos_AdicionRetiro_Activos_Obtener(CodEmpresa);
        }
        public ErrorDto Activos_AdicionRetiro_Guardar(int CodEmpresa, string usuario, ActivosRetiroAdicionData data)
        {
            return _db.Activos_AdicionRetiro_Guardar(CodEmpresa, usuario, data);
        }
        public ErrorDto<List<ActivosHistoricoData>> Activos_AdicionRetiro_Historico_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_AdicionRetiro_Historico_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<List<ActivosRetiroAdicionCierreData>> Activos_AdicionRetiro_Cierres_Consultar(int CodEmpresa, string placa, int Id_AddRet)
        {
            return _db.Activos_AdicionRetiro_Cierres_Consultar(CodEmpresa, placa, Id_AddRet);
        }
        public ErrorDto<string?> Activos_AdicionRetiro_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_AdicionRetiro_ActivosNombre_Consultar(CodEmpresa, placa);
        }
        public ErrorDto Activos_AdicionRetiro_Eliminar(int CodEmpresa, string placa, int Id_AddRet)
        {
            return _db.Activos_AdicionRetiro_Eliminar(CodEmpresa, placa, Id_AddRet);
        }
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }
    }
}