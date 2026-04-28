using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysPortalBL(IConfiguration config)
    {
        private readonly FrmSysPortalDB _db = new FrmSysPortalDB(config);

        public ErrorDto<SysMensajesPortalLista> Sys_MensajesPortal_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sys_MensajesPortal_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<SysMensajesPortalListaItem>> Sys_MensajesPortal_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sys_MensajesPortal_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<SysMensajesPortalDetalleModel> Sys_MensajesPortal_Detalle_Obtener(int CodEmpresa, string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return new ErrorDto<SysMensajesPortalDetalleModel> { Code = 1, Description = "Código requerido." };

            return _db.Sys_MensajesPortal_Detalle_Obtener(CodEmpresa, codigo.Trim());
        }

        public ErrorDto Sys_MensajesPortal_Mensaje_Guardar(int CodEmpresa, SysMensajesPortalDetalleModel dto, string usuario)
        {
            var validationError = ValidateMensajePortalDto(dto);
            if (validationError != null)
                return validationError;

            NormalizeMensajePortalDto(dto);

            var activationError = ValidateAndNormalizeActivacion(dto);
            if (activationError.Code != 0)
                return activationError;

            return _db.Sys_MensajesPortal_Mensaje_Guardar(CodEmpresa, dto, usuario?.Trim() ?? string.Empty);
        }

        private static ErrorDto? ValidateMensajePortalDto(SysMensajesPortalDetalleModel dto)
        {
            if (dto == null) return new ErrorDto { Code = 1, Description = "Payload vacío." };
            if (string.IsNullOrWhiteSpace(dto.codigo)) return new ErrorDto { Code = 1, Description = "El código es obligatorio." };
            if (string.IsNullOrWhiteSpace(dto.titulo)) return new ErrorDto { Code = 1, Description = "El título es obligatorio." };
            if (string.IsNullOrWhiteSpace(dto.tipo_formato_cod)) return new ErrorDto { Code = 1, Description = "Debe seleccionar un tipo de formato." };

            return null;
        }

        private static void NormalizeMensajePortalDto(SysMensajesPortalDetalleModel dto)
        {
            dto.codigo = dto.codigo.Trim();
            dto.titulo = dto.titulo.Trim();
            dto.smtp_id = dto.smtp_id?.Trim() ?? string.Empty;
            dto.tipo_formato_cod = dto.tipo_formato_cod.Trim();
            dto.pie_01 = dto.pie_01?.Trim() ?? string.Empty;
            dto.pie_02 = dto.pie_02?.Trim() ?? string.Empty;
            dto.procedimiento = dto.procedimiento?.Trim() ?? string.Empty;
            dto.imagen_ruta = dto.imagen_ruta?.Trim() ?? string.Empty;
            dto.evento_codigo = string.IsNullOrWhiteSpace(dto.evento_codigo) ? null : dto.evento_codigo.Trim();
            dto.imagen_ancho = dto.imagen_ancho <= 0 ? 600 : dto.imagen_ancho;
            dto.imagen_alto = dto.imagen_alto <= 0 ? 300 : dto.imagen_alto;
        }

        private static ErrorDto ValidateAndNormalizeActivacion(SysMensajesPortalDetalleModel dto)
        {
            switch (char.ToUpperInvariant(dto.activacion))
            {
                case 'M':
                    SetActivacionManual(dto);
                    return new ErrorDto { Code = 0, Description = "OK" };

                case 'F':
                    return ValidateActivacionFecha(dto);

                case 'D':
                    return ValidateActivacionDia(dto);

                case 'C':
                    return ValidateActivacionFrecuencia(dto);

                case 'E':
                    return ValidateActivacionEvento(dto);

                default:
                    return new ErrorDto { Code = 1, Description = "Código de activación inválido. Valores permitidos: M,F,D,C,E." };
            }
        }

        private static void SetActivacionManual(SysMensajesPortalDetalleModel dto)
        {
            dto.activacion = 'M';
            dto.fecha_especifica = null;
            dto.dia_del_mes = 1;
            dto.frecuencia_n_dias = 7;
            dto.frecuencia_inicio = null;
            dto.evento_codigo = "N/A";
        }

        private static ErrorDto ValidateActivacionFecha(SysMensajesPortalDetalleModel dto)
        {
            if (dto.fecha_especifica == null)
                return new ErrorDto { Code = 1, Description = "Debe indicar la fecha específica." };

            dto.dia_del_mes = 1;
            dto.frecuencia_n_dias = 7;
            dto.frecuencia_inicio = null;
            dto.evento_codigo = "N/A";

            return new ErrorDto { Code = 0, Description = "OK" };
        }

        private static ErrorDto ValidateActivacionDia(SysMensajesPortalDetalleModel dto)
        {
            if (dto.dia_del_mes == null || dto.dia_del_mes < 1 || dto.dia_del_mes > 32)
                return new ErrorDto { Code = 1, Description = "El día del mes debe estar entre 1 y 31, o 32 para 'Último día'." };

            dto.fecha_especifica = null;
            dto.frecuencia_n_dias = 7;
            dto.frecuencia_inicio = null;
            dto.evento_codigo = "N/A";

            return new ErrorDto { Code = 0, Description = "OK" };
        }

        private static ErrorDto ValidateActivacionFrecuencia(SysMensajesPortalDetalleModel dto)
        {
            if (dto.frecuencia_n_dias == null || dto.frecuencia_n_dias < 1 || dto.frecuencia_n_dias > 32)
                return new ErrorDto { Code = 1, Description = "La frecuencia (N días) debe estar entre 1 y 32." };

            if (dto.frecuencia_inicio == null)
                return new ErrorDto { Code = 1, Description = "Debe indicar la fecha de inicio de la frecuencia." };

            dto.fecha_especifica = null;
            dto.dia_del_mes = 1;
            dto.evento_codigo = "N/A";

            return new ErrorDto { Code = 0, Description = "OK" };
        }

        private static ErrorDto ValidateActivacionEvento(SysMensajesPortalDetalleModel dto)
        {
            if (string.IsNullOrWhiteSpace(dto.evento_codigo))
                return new ErrorDto { Code = 1, Description = "Debe seleccionar un evento para la activación por evento." };

            dto.fecha_especifica = null;
            dto.dia_del_mes = 1;
            dto.frecuencia_n_dias = 7;
            dto.frecuencia_inicio = null;
            dto.evento_codigo = dto.evento_codigo.Trim();

            return new ErrorDto { Code = 0, Description = "OK" };
        }

        public ErrorDto Sys_MensajesPortal_Mensaje_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return new ErrorDto { Code = 1, Description = "Código requerido." };

            return _db.Sys_MensajesPortal_Mensaje_Eliminar(CodEmpresa, codigo.Trim(), usuario?.Trim() ?? string.Empty);
        }

        public ErrorDto<List<SysMensajesPortalSmtpDto>> Sys_MensajesPortal_Smtps_Obtener(int CodEmpresa)
        {
            return _db.Sys_MensajesPortal_Smtps_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SysMensajesPortalFormatoDto>> Sys_MensajesPortal_Formatos_Obtener(int CodEmpresa)
        {
            return _db.Sys_MensajesPortal_Formatos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SysMensajesPortalActivacionDto>> Sys_MensajesPortal_Activaciones_Obtener()
        {
            return _db.Sys_MensajesPortal_Activaciones_Obtener();
        }

        public ErrorDto<List<SysMensajesPortalEventoDto>> Sys_MensajesPortal_Eventos_Obtener(int CodEmpresa)
        {
            return _db.Sys_MensajesPortal_Eventos_Obtener(CodEmpresa);
        }

        public ErrorDto<SysMensajesPortalPreferenciasModel> Sys_MensajesPortal_Portal_Obtener(int CodEmpresa)
        {
            return _db.Sys_MensajesPortal_Portal_Obtener(CodEmpresa);
        }

        public ErrorDto Sys_MensajesPortal_Portal_Guardar(int CodEmpresa, SysMensajesPortalPreferenciasModel dto, string usuario)
        {
            if (dto == null) return new ErrorDto { Code = 1, Description = "Payload vacío." };

            dto.logo_url = dto.logo_url?.Trim() ?? string.Empty;

            if (dto.logo_alto < 0 || dto.logo_ancho < 0)
                return new ErrorDto { Code = 1, Description = "Las dimensiones del logo no pueden ser negativas." };

            dto.color_set_hex = dto.color_set_hex?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(dto.color_set_hex) && dto.color_set_hex.Length != 6)
                return new ErrorDto { Code = 1, Description = "El color base debe ser un hex de 6 caracteres (sin #)." };

            return _db.Sys_MensajesPortal_Portal_Guardar(CodEmpresa, dto, usuario?.Trim() ?? string.Empty);
        }
    }
}