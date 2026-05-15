using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndSeguridadMovimientosBL
    {
        private readonly FrmFndSeguridadMovimientosDB _Db;

        public FrmFndSeguridadMovimientosBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndSeguridadMovimientosDB(config);
        }

        // ===================== PLANES =====================
        public ErrorDto<List<SeguridadMovimientoPlanDto>> Seguridad_Planes_Obtener(
            int CodEmpresa, string cod_grupo)
        {
            return _Db.Seguridad_Planes_Obtener(CodEmpresa, cod_grupo);
        }

        public ErrorDto<bool> Seguridad_Planes_Marcar(
            int CodEmpresa, string cod_grupo, string cod_plan,
            int cod_operadora, bool marcado, string usuario)
        {
            return _Db.Seguridad_Planes_Marcar(
                CodEmpresa, cod_grupo, cod_plan, cod_operadora, marcado, usuario);
        }

        // ===================== USUARIOS =====================
        public ErrorDto<List<SeguridadMovimientoUsuarioDto>> Seguridad_Usuarios_Obtener(
            int CodEmpresa, string cod_grupo)
        {
            return _Db.Seguridad_Usuarios_Obtener(CodEmpresa, cod_grupo);
        }

        public ErrorDto<bool> Seguridad_Usuarios_Marcar(
            int CodEmpresa, string cod_grupo, string usuarioMarcado, bool marcado, string usuario)
        {
            return _Db.Seguridad_Usuarios_Marcar(CodEmpresa, cod_grupo, usuarioMarcado, marcado, usuario);
        }

        // ===================== AUTORIZADORES =====================
        public ErrorDto<List<SeguridadMovimientoUsuarioDto>> Seguridad_Autorizadores_Obtener(
            int CodEmpresa, string cod_grupo)
        {
            return _Db.Seguridad_Autorizadores_Obtener(CodEmpresa, cod_grupo);
        }

        public ErrorDto<bool> Seguridad_Autorizadores_Marcar(
            int CodEmpresa, string cod_grupo, string usuarioMarcado, bool marcado, string usuario)
        {
            return _Db.Seguridad_Autorizadores_Marcar(
                CodEmpresa, cod_grupo, usuarioMarcado, marcado, usuario);
        }

        public ErrorDto<List<SeguridadMovimientoNivelDto>> Seguridad_Niveles_Obtener(int CodEmpresa)
        {
            return _Db.Seguridad_Niveles_Obtener(CodEmpresa);
        }

        public ErrorDto<bool> Seguridad_Niveles_Guardar(int CodEmpresa, SeguridadMovimientoNivelDto dto, string usuario)
        {
            return _Db.Seguridad_Niveles_Guardar(CodEmpresa, dto, usuario);
        }

        public ErrorDto<bool> Seguridad_Niveles_Eliminar(int CodEmpresa, string cod_grupo, string usuario)
        {
            return _Db.Seguridad_Niveles_Eliminar(CodEmpresa, cod_grupo, usuario);
        }

    }
}