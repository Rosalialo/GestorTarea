# Políticas de Acceso a Usuarios

Este documento define los roles y permisos para el equipo de trabajo.

---

## Roles y Permisos

### 1. Líder del Proyecto

- **Permisos:**
  - Puede hacer **push directo** a las ramas protegidas (ej. `main`, `release/*`).
  - Aprueba y fusiona Pull Requests (PR).
  - Gestiona las ramas y versiones.

- **Flujo de trabajo:**
  - Trabaja tanto directamente en ramas protegidas como mediante PR para revisiones importantes.

---

### 2. Desarrollador

- **Permisos:**
  - No puede hacer push directo a ramas protegidas.
  - Debe trabajar usando **Pull Requests** para fusionar código.
  - Puede crear y gestionar ramas de tipo `feature/` y `bugfix/`.

- **Flujo de trabajo:**
  - Crea ramas locales.
  - Envía cambios mediante PR para revisión y aprobación.

---

### 3. Colaborador Externo

- **Permisos:**
  - Solo puede trabajar mediante **Pull Requests**.
  - No tiene permisos para hacer push directo a ninguna rama protegida.
  - Solo puede acceder a ramas no protegidas o forks.

- **Flujo de trabajo:**
  - Realiza contribuciones mediante PR.
  - El líder o desarrollador revisa y aprueba los PR.

---

## Resumen de Accesos

| Rol                | Push directo a ramas protegidas | Trabaja con Pull Requests | Gestión de ramas     |
|--------------------|---------------------------------|---------------------------|---------------------|
| Líder del Proyecto  | Sí                              | Sí                        | Sí                  |
| Desarrollador      | No                              | Sí                        | Sí (feature/bugfix) |
| Colaborador Externo | No                              | Sí                        | No                  |

---

*Estas políticas ayudan a mantener la calidad del código y controlar los cambios en producción.*
