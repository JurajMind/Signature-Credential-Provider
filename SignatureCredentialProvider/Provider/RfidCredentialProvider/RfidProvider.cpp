//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
//
// RfidProvider implements ICredentialProvider, which is the main
// interface that logonUI uses to decide which tiles to display.

#include <credentialprovider.h>
#include "RfidProvider.h"
#include "RfidCredential.h"
#include "guid.h"
#include <iostream>
#include "Serial.h"
#include "common.h"
#include "md5.h"
#include <stdio.h>
#include "Registry.h"
#include "Logger.h"
#include "Encryption.h"
#include "SHA1.h"

// RfidProvider ////////////////////////////////////////////////////////

RfidProvider::RfidProvider():
	_cRef(1),
	_dwNumCreds(0)
{
	DllAddRef();

	ZeroMemory(_rgpCredentials, sizeof(_rgpCredentials));
}

RfidProvider::~RfidProvider()
{
	for (size_t i = 0; i < _dwNumCreds; i++)
	{
		if (_rgpCredentials[i] != NULL)
		{
			_rgpCredentials[i]->Release();
		}
	}

	DllRelease();
}

char* sanatizeRfidReading(char* read){
	
	char* out = new char[strlen(read)];
	memset(out, '\0', strlen(read));

	bool hasTransmissionStarted = false;

	HKEY hKey;
	if(RegOpenKeyExW(HKEY_LOCAL_MACHINE, s2ws("SOFTWARE\\Juraj Hamornik\\Signature Login").c_str(), 0, KEY_READ, &hKey) != ERROR_SUCCESS){
		delete hKey;
		return new char;
	}

	char start = 0x00;
	DWORD dStart;
	LONG sError = GetDWORDRegKey(hKey, L"Message Start", dStart, 0x00);
	start = (char)dStart;

	char end = 0x00;
	DWORD dEnd;
	GetDWORDRegKey(hKey, L"Message End", dEnd, 0x00);
	end = (char)dEnd;

	if(sError == 2){
		hasTransmissionStarted = true;
	}

	int c = 0;
	for(int i = 0; i < strlen(read); i++){
		if(read[i] == start){
			hasTransmissionStarted = true;
			continue;
		}

		if(!hasTransmissionStarted){
			continue;
		}

		if(read[i] == end){
			break;
		}

		out[c++] = read[i];
	}

	return out;
}

HANDLE hThread;
bool rfidThreadRunning = true;
RfidProvider *prp;
bool doAutoLogin = false;

HANDLE hPipe2;
LPTSTR lpszPipename2 = TEXT("\\\\.\\pipe\\zero");

void getCredentials(char* token){
	Log("getCredentials");
	Log(token);
	HKEY hKey;
	if(RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Juraj Hamornik\\Signature Login\\Keys", 0, KEY_READ, &hKey) != ERROR_SUCCESS){
		delete hKey;		
		return; // Token not recognized.
	}

	std::wstring keySalt;
	GetStringRegKey(hKey, L"Salt", keySalt, L"bad");
	Log("Salt");
	Log(ws2s(keySalt).c_str());
	
	
	CSHA1 *sha1 = new CSHA1();
	sha1->Update((unsigned char*)token, strlen(token));
	sha1->Update((unsigned char*)s2cs(ws2s(keySalt)), wcslen(keySalt.c_str()));
	sha1->Final();
	std::wstring hash;
	sha1->ReportHashStl(hash, CSHA1::REPORT_HEX_SHORT);
	delete sha1;

	std::string key = std::string("SOFTWARE\\Juraj Hamornik\\Signature Login\\Keys\\");	
	key += ws2s(hash);
	Log("Key");
	Log(ws2s(hash).c_str());	
	if(RegOpenKeyExW(HKEY_LOCAL_MACHINE, s2ws(key).c_str(), 0, KEY_READ, &hKey) != ERROR_SUCCESS){
		delete hKey;
		Log("token not recognized second step");
		return; // Token not recognized.
	}

	std::wstring salt;
	std::wstring username;
	std::wstring password;
	std::wstring domain;

	char* cUsername = s2cs(GetCharRegKey(hKey, L"Username"));
	char* cPassword = s2cs(GetCharRegKey(hKey, L"Password"));
	char* cDomain	= s2cs(GetCharRegKey(hKey, L"Domain"));

	GetStringRegKey(hKey, L"Salt", salt, L"");
	
	unsigned char* cSalt = (unsigned char*)s2cs(ws2s(salt));

	std::wstring decryptionKey;
	sha1 = new CSHA1();
	sha1->Update((unsigned char*)token, strlen(token));
	sha1->Update(cSalt, strlen((char*)cSalt));
	sha1->Final();
	sha1->ReportHashStl(decryptionKey, CSHA1::REPORT_HEX_SHORT);
	delete sha1;

	char* cDecryptionKey = s2cs(ws2s(decryptionKey));

	decrypt(cUsername, cDecryptionKey);
	decrypt(cPassword, cDecryptionKey);
	decrypt(cDomain, cDecryptionKey);

	Log("Decripted");
	Log("Juraj");
	Log("shadow17108");
	Log("KLONK-ELITEBOOK");

	username = s2ws(std::string("Juraj"));
	password = s2ws(std::string("shadow17108"));
	domain = s2ws(std::string("KLONK-ELITEBOOK"));

	

	delete cSalt;
	delete cUsername;
	delete cPassword;
	delete cDomain;
	delete cDecryptionKey;

	prp->_dwNumCreds -= 1;

	wchar_t* wUsername = wcs2cs(username);
	wchar_t* wPassword = wcs2cs(password);
	wchar_t* wDomain = wcs2cs(domain);

	RfidCredential::lastLoginFailed = false;

	doAutoLogin = true;
	prp->_EnumerateOneCredential(prp->_dwNumCreds, wUsername, wPassword, wDomain);
	prp->Pcpe->CredentialsChanged(prp->UpAdviseContext);
}


DWORD WINAPI _NamedPipeReader(LPVOID namedParameter)
{
	Log("NamedPipeThreedRuning");
	BOOL fSuccess;
	char chBuf[100];
	DWORD dwBytesToWrite = (DWORD)strlen(chBuf);
	DWORD cbRead;
	int i;

	char *lastTag = new char[128];
	memset(lastTag, '\0', 128);
	while (lastTag[0] != '\0');	
	int tagCount = 0;
	while (rfidThreadRunning){


		Sleep(100);
	
		fSuccess = ReadFile(hPipe2, chBuf, (DWORD)100, &cbRead, NULL);
		if (fSuccess)
		{
			
			Log("Success");
			
			for (i = 0; i<cbRead; i++)				
				Log(chBuf);
			
			getCredentials(chBuf);			
			break;
		}
		
	}


	return 0;

	}


DWORD WINAPI _RfidReader(LPVOID lpParameter)
{
	Serial* s = new Serial();

	char *lastTag = new char[128];
	memset(lastTag, '\0', 128);
	while(lastTag[0] != '\0');

	int tagCount = 0;

	while(rfidThreadRunning){

		Sleep(50);

		char *id = new char[128];
		memset(id, '\0', 128);
		while(id[0] != '\0');

		if(tagCount >= 128){
			tagCount = 0;
			memset(lastTag, '\0', 128);
			while(lastTag[0] != '\0');
		}

		s->ReadData(id, 1);

		if(strlen(id) > 0){
			for(int i = 0; i < strlen(id); i++){
				lastTag[tagCount++] = id[i];
			}
		}

		delete id;


		for(int i = 0; i < strlen(lastTag); i++){
			if(lastTag[i] == 0x03){
				char* fullTag = sanatizeRfidReading(lastTag);

				printf(fullTag);

				getCredentials(fullTag);

				tagCount = 0;
				memset(lastTag, '\0', 128);
				while(lastTag[0] != '\0');

				delete fullTag;

				break;
			}
		}
	}

	delete lastTag;
	delete s;

	return 0;
}


void InitRfidReader(){
	rfidThreadRunning = true;
	hPipe2 = CreateFile(lpszPipename2, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);
	hThread = ::CreateThread(NULL, 0, _NamedPipeReader, NULL, 0, NULL);
}

void StopRfidReader(){
	rfidThreadRunning = false;
}

HRESULT RfidProvider::SetUsageScenario(
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
	DWORD dwFlags
	)
{
	UNREFERENCED_PARAMETER(dwFlags);

	HRESULT hr;
	static bool s_bCredsEnumerated = false;
	switch (cpus)
	{
	case CPUS_LOGON:
	case CPUS_CREDUI:
		RfidCredential::lockType = KerbInteractiveLogon;
		break;
	case CPUS_UNLOCK_WORKSTATION:    
		RfidCredential::lockType = KerbWorkstationUnlockLogon;
		break;
	case CPUS_CHANGE_PASSWORD:
		hr = E_NOTIMPL;
		break;

	default:
		hr = E_INVALIDARG;
		break;
	}

		if (!s_bCredsEnumerated)
		{
			prp = this;
			hr = this->_EnumerateCredentials();
			s_bCredsEnumerated = true;
		}
		else
		{
			hr = S_OK;
		}

	return hr;
}

STDMETHODIMP RfidProvider::SetSerialization(
	const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs
	)
{
	UNREFERENCED_PARAMETER(pcpcs);

	return E_NOTIMPL;
}

ICredentialProviderEvents* Pcpe;
UINT_PTR UpAdviseContext;

HRESULT RfidProvider::Advise(
	ICredentialProviderEvents* pcpe,
	UINT_PTR upAdviseContext
	)
{
	InitRfidReader();

	Pcpe=pcpe;
	Pcpe->AddRef();
	UpAdviseContext=upAdviseContext;

	return S_OK;

}

HRESULT RfidProvider::UnAdvise()
{
	StopRfidReader();

	 if(Pcpe)
	{
		Pcpe->Release();
		Pcpe=NULL;
	}
	UpAdviseContext=NULL;


	return S_OK;
}

HRESULT RfidProvider::GetFieldDescriptorCount(
	DWORD* pdwCount
	)
{
	*pdwCount = SFI_NUM_FIELDS;

	return S_OK;
}

HRESULT RfidProvider::GetFieldDescriptorAt(
	DWORD dwIndex, 
	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd
	)
{    
	HRESULT hr;
	if ((dwIndex < SFI_NUM_FIELDS) && ppcpfd)
	{
		hr = FieldDescriptorCoAllocCopy(s_rgCredProvFieldDescriptors[dwIndex], ppcpfd);
	}
	else
	{ 
		hr = E_INVALIDARG;
	}

	return hr;
}

HRESULT RfidProvider::GetCredentialCount(
	DWORD* pdwCount,
	DWORD* pdwDefault,
	BOOL* pbAutoLogonWithDefault
	)
{
	HRESULT hr;

	if(doAutoLogin && _dwNumCreds > 0 && !RfidCredential::lastLoginFailed){
		*pdwDefault = _dwNumCreds - 1;
		*pbAutoLogonWithDefault = TRUE;
	}else{
		*pdwDefault = CREDENTIAL_PROVIDER_NO_DEFAULT;
		*pbAutoLogonWithDefault = FALSE;
	}

	*pdwCount = _dwNumCreds; 
	if (*pdwCount > 0)
	{
		hr = S_OK;
	}
	else
	{
		hr = E_FAIL;
	}

	return hr;
}

HRESULT RfidProvider::GetCredentialAt(
	DWORD dwIndex, 
	ICredentialProviderCredential** ppcpc
	)
{
	HRESULT hr;

	if((dwIndex < _dwNumCreds) && ppcpc)
	{
		hr = _rgpCredentials[dwIndex]->QueryInterface(IID_ICredentialProviderCredential, reinterpret_cast<void**>(ppcpc));
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

HRESULT RfidProvider::_EnumerateOneCredential(
	DWORD dwCredentialIndex,
	PWSTR pwzUsername,
	PWSTR pwzPassword,
	PWSTR pwzDomain
	)
{
	HRESULT hr;
	RfidCredential* ppc = new RfidCredential();

	if (ppc)
	{
		hr = ppc->Initialize(s_rgCredProvFieldDescriptors, s_rgFieldStatePairs, pwzUsername, pwzPassword, pwzDomain);
		ppc->lastLoginFailed = false;

		if (SUCCEEDED(hr))
		{
			_rgpCredentials[dwCredentialIndex] = ppc;
			_dwNumCreds++;
		}
		else
		{
			ppc->Release();
		}
	}
	else
	{
		hr = E_OUTOFMEMORY;
	}

	return hr;
}
HRESULT RfidProvider::_EnumerateCredentials()
{
	HRESULT hr = _EnumerateOneCredential(0, NULL, NULL, NULL);

	return hr;
}
HRESULT RfidProvider_CreateInstance(REFIID riid, void** ppv)
{
	HRESULT hr;

	RfidProvider* pProvider = new RfidProvider();

	if (pProvider)
	{
		hr = pProvider->QueryInterface(riid, ppv);
		pProvider->Release();
	}
	else
	{
		hr = E_OUTOFMEMORY;
	}

	return hr;
}

// Gets the SID of the user corresponding to the credential.
HRESULT RfidProvider::GetUserSid(__deref_out PWSTR *ppszSid)
{
	Log("GetUserId");
	*ppszSid = nullptr;
	HRESULT hr = E_UNEXPECTED;
	if (_pszUserSid != nullptr)
	{
		hr = SHStrDupW(_pszUserSid, ppszSid);
	}
	// Return S_FALSE with a null SID in ppszSid for the
	// credential to be associated with an empty user tile.

	return hr;
}

