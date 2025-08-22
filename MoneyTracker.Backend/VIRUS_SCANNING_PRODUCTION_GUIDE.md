# Production Virus Scanning Configuration

## Overview
The PDF Upload System includes a comprehensive virus scanning service that supports multiple antivirus providers for production environments.

## Supported Antivirus Providers

### 1. ClamAV (Recommended for Self-Hosted)
**Best for**: On-premises deployments, privacy-sensitive environments

```json
{
  "VirusScan": {
    "Enabled": true,
    "Provider": "clamav",
    "ClamAV": {
      "Endpoint": "localhost:3310"
    }
  }
}
```

**Setup Instructions**:
1. Install ClamAV daemon
2. Configure TCP interface on port 3310
3. Ensure regular virus definition updates
4. Install ClamAV client library: `nClamAV` NuGet package

### 2. VirusTotal (Recommended for Cloud)
**Best for**: Cloud deployments, multiple engine scanning

```json
{
  "VirusScan": {
    "Enabled": true,
    "Provider": "virustotal",
    "VirusTotal": {
      "ApiKey": "your-virustotal-api-key"
    }
  }
}
```

**Setup Instructions**:
1. Register at https://www.virustotal.com/
2. Obtain API key from your account
3. Note: Free tier has rate limits
4. Consider paid tier for production usage

### 3. OPSWAT Metadefender
**Best for**: Enterprise environments, compliance requirements

```json
{
  "VirusScan": {
    "Enabled": true,
    "Provider": "metadefender",
    "Metadefender": {
      "ApiKey": "your-metadefender-api-key"
    }
  }
}
```

**Setup Instructions**:
1. Register at https://metadefender.opswat.com/
2. Obtain API key
3. Configure rate limits based on your plan

### 4. Windows Defender (Windows Only)
**Best for**: Windows-hosted applications

```json
{
  "VirusScan": {
    "Enabled": true,
    "Provider": "windows_defender"
  }
}
```

**Setup Instructions**:
1. Ensure Windows Defender is installed and updated
2. Verify MpCmdRun.exe is available in system PATH
3. Grant application permissions to execute antivirus scans

### 5. Mock Scanner (Development Only)
**Best for**: Development and testing environments

```json
{
  "VirusScan": {
    "Enabled": true,
    "Provider": "mock"
  }
}
```

## Implementation Steps

### Step 1: Choose Your Provider
Select the antivirus provider that best fits your environment and requirements.

### Step 2: Update Configuration
Add the appropriate configuration to your `appsettings.json` or environment variables.

### Step 3: Install Dependencies
Install any required NuGet packages or external software.

### Step 4: Register Service
Update your dependency injection configuration:

```csharp
// In Infrastructure/DependencyInjection.cs
services.AddScoped<IVirusScanService, ProductionVirusScanService>();
services.AddHttpClient<ProductionVirusScanService>();
```

### Step 5: Update File Storage Service
Replace the mock implementation in `LocalFileStorageService.cs`:

```csharp
public async Task PerformVirusScanAsync(byte[] fileContent)
{
    var virusScanService = _serviceProvider.GetRequiredService<IVirusScanService>();
    var result = await virusScanService.ScanFileAsync(fileContent, "uploaded-file");
    
    if (!result.IsClean)
    {
        throw new SecurityException($"Virus detected: {result.Message}");
    }
}
```

## Security Considerations

### API Key Management
- Store API keys in secure configuration (Azure Key Vault, AWS Secrets Manager)
- Never commit API keys to source control
- Use environment variables in production
- Rotate API keys regularly

### Rate Limiting
- Configure appropriate timeouts for virus scanning
- Implement retry logic for temporary failures
- Monitor API usage to avoid rate limits

### Error Handling
- Always fail securely (reject files if scan fails)
- Log all virus scan results for auditing
- Implement fallback scanning if primary service fails

### Performance
- Consider async processing for large files
- Implement caching for frequently uploaded files
- Set appropriate timeout values

## Monitoring and Logging

### Essential Metrics
- Scan success/failure rates
- Average scan duration
- Virus detection counts
- API rate limit usage

### Log Events
- All virus scan results
- Failed scans with error details
- Detected threats with file metadata
- API rate limit warnings

## Cost Considerations

| Provider | Free Tier | Paid Plans | Best For |
|----------|-----------|------------|----------|
| ClamAV | Free | Self-hosted costs | High volume, privacy |
| VirusTotal | 4 req/min | $50+/month | Moderate volume |
| Metadefender | Limited | Enterprise pricing | Compliance requirements |
| Windows Defender | Included with Windows | - | Windows environments |

## Testing

### EICAR Test File
Use the EICAR test file to verify your virus scanning implementation:

```
X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*
```

This harmless test file should be detected as a "virus" by any properly configured antivirus system.

## Troubleshooting

### Common Issues
1. **API Key Issues**: Verify key is valid and has sufficient quota
2. **Network Issues**: Check firewall settings and proxy configuration
3. **Timeout Issues**: Increase timeout values for large files
4. **Permission Issues**: Ensure application has necessary permissions

### Debug Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "MoneyTracker.Infrastructure.Services.ProductionVirusScanService": "Debug"
    }
  }
}
```

---

**Important**: The current mock implementation should be replaced with a production antivirus service before deploying to production environments. File security is critical for preventing malware uploads.