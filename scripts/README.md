# 📁 Scripts Directory

This directory contains all system scripts organized by purpose.

## 📋 Directory Structure

```
scripts/
├── docker/                 # Docker deployment scripts
│   ├── start-oms.sh       # 🚀 Main Docker startup script
│   └── test-integration.sh # 🧪 Integration testing
├── local/                  # Local development scripts  
│   ├── start-local.sh     # 🏠 Start local development
│   ├── stop-local.sh      # 🛑 Stop local services
│   ├── check-local.sh     # 🔍 Check local system status
│   └── check-rabbitmq.sh  # 🐰 RabbitMQ diagnostics
├── diagnostics/            # System diagnostic scripts
│   ├── diagnose-system.sh # 🔧 System diagnostics (Linux/Mac)
│   └── diagnose-system.ps1 # 🔧 System diagnostics (Windows)
└── optional/               # Optional/advanced scripts
    ├── start-system.sh    # Advanced Docker startup (Linux/Mac)
    ├── start-system.ps1   # Advanced Docker startup (Windows)
    └── run-all.sh         # Legacy startup script
```

## 🚀 Quick Commands

### Docker Deployment
```bash
# Start complete system with Docker
bash scripts/docker/start-oms.sh

# Run integration tests
bash scripts/docker/test-integration.sh
```

### Local Development
```bash
# Start local development environment
bash scripts/local/start-local.sh

# Check system status
bash scripts/local/check-local.sh

# Stop local services
bash scripts/local/stop-local.sh
```

### Diagnostics
```bash
# Run system diagnostics
bash scripts/diagnostics/diagnose-system.sh

# Check RabbitMQ specifically
bash scripts/local/check-rabbitmq.sh
```

## 📝 Script Descriptions

| Script | Purpose | Environment |
|--------|---------|-------------|
| `docker/start-oms.sh` | **Main deployment script** - Starts complete system | Docker |
| `docker/test-integration.sh` | Tests all services integration | Docker |
| `local/start-local.sh` | Starts services for local development | Local |
| `local/stop-local.sh` | Stops local development services | Local |
| `local/check-local.sh` | Verifies local system health | Local |
| `local/check-rabbitmq.sh` | Diagnoses RabbitMQ issues | Local |
| `diagnostics/diagnose-system.*` | System health diagnostics | Any |

## 🎯 Recommended Usage

### For Demos/Production
```bash
bash scripts/docker/start-oms.sh
```

### For Development
```bash
bash scripts/local/start-local.sh
```

### For Troubleshooting
```bash
bash scripts/local/check-local.sh
bash scripts/diagnostics/diagnose-system.sh
```

---

**📋 All scripts are organized by purpose for better maintainability and understanding.**