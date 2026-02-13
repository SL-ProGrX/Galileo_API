using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosSuscripcionesBL
    {
        private readonly FrmCxCContratosSuscripcionesDB _db;

        public FrmCxCContratosSuscripcionesBL(IConfiguration config)
        {
            _db = new FrmCxCContratosSuscripcionesDB(config);
        }

        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa)
        {
            return _db.CxcPersonas_Lista(codEmpresa);
        }

        public ErrorDto<CxcPersonaContratoDto?> CxcPersonaContrato_Obtener(int codEmpresa, string cedula, string codContrato)
        {
            return _db.CxcPersonaContrato_Obtener(codEmpresa, cedula, codContrato);
        }

        public ErrorDto<bool> CxcPersonaContrato_Guardar(int codEmpresa, CxcPersonaContratoSaveParams param)
        {
            return _db.CxcPersonaContrato_Guardar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcPersonaContrato_Eliminar(int codEmpresa, CxcPersonaContratoDeleteParams param)
        {
            return _db.CxcPersonaContrato_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CxcPersonaContratoPagadorDto>> CxcPersonaContratoPagadores_Lista(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxcPersonaContratoPagadores_Lista(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<List<CxcPersonaContratoPagadorDto>> CxcContratoPagadoresDisponibles_Lista(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxcContratoPagadoresDisponibles_Lista(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<bool> CxcPersonaContratoPagador_Insertar(int codEmpresa, CxcPersonaContratoPagadorSaveParams param)
        {
            return _db.CxcPersonaContratoPagador_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcPersonaContratoPagador_Eliminar(int codEmpresa, CxcPersonaContratoPagadorDeleteParams param)
        {
            return _db.CxcPersonaContratoPagador_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CxcPersonaContratoSuscripcionDto>> CxcPersonaContratoSuscripciones_Lista(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxcPersonaContratoSuscripciones_Lista(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<List<CxcPersonaContratoSuscripcionDto>> CxcContratoCargosDisponibles_Lista(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxcContratoCargosDisponibles_Lista(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<bool> CxcPersonaContratoSuscripcion_Insertar(int codEmpresa, CxcPersonaContratoSuscripcionSaveParams param)
        {
            return _db.CxcPersonaContratoSuscripcion_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcPersonaContratoSuscripcion_Eliminar(int codEmpresa, CxcPersonaContratoSuscripcionDeleteParams param)
        {
            return _db.CxcPersonaContratoSuscripcion_Eliminar(codEmpresa, param);
        }
    }
}
