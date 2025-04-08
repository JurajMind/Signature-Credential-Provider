//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// CSampleProvider implements ICredentialProvider, which is the main
// interface that logonUI uses to decide which tiles to display.
// In this sample, we will display one tile that uses each of the nine
// available UI controls.

#include <tchar.h>
#include <initguid.h>
#include "CSampleProvider.h"
#include "CSampleCredential.h"
#include "guid.h"
#include "Logger.h"
#include "SHA1.h"
#include "Encryption.h"
#include "Registry.h"
#include <stdio.h>
#include <conio.h>
#include <strsafe.h>
#include <iostream>

CSampleProvider::CSampleProvider() :
	_cRef(1),
	_pCredential(nullptr),
	_pCredProviderUserArray(nullptr)
{
	DllAddRef();
}

CSampleProvider::~CSampleProvider()
{
	if (_pCredential != nullptr)
	{
		_pCredential->Release();
		_pCredential = nullptr;
	}
	if (_pCredProviderUserArray != nullptr)
	{
		_pCredProviderUserArray->Release();
		_pCredProviderUserArray = nullptr;
	}

	DllRelease();
}

#define BUF_SIZE 256
TCHAR szName[] = TEXT("Global\\MyFileMappingObject");
TCHAR szMsg[] = TEXT("Message from first process.");

// SetUsageScenario is the provider's cue that it's going to be asked for tiles
// in a subsequent call.
HRESULT CSampleProvider::SetUsageScenario(
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
	DWORD /*dwFlags*/)
{
	HRESULT hr;

	// Decide which scenarios to support here. Returning E_NOTIMPL simply tells the caller
	// that we're not designed for that scenario.
	switch (cpus)
	{
	case CPUS_LOGON:
	case CPUS_UNLOCK_WORKSTATION:
		// The reason why we need _fRecreateEnumeratedCredentials is because ICredentialProviderSetUserArray::SetUserArray() is called after ICredentialProvider::SetUsageScenario(),
		// while we need the ICredentialProviderUserArray during enumeration in ICredentialProvider::GetCredentialCount()
		_cpus = cpus;
		_fRecreateEnumeratedCredentials = true;
		hr = S_OK;
		break;

	case CPUS_CHANGE_PASSWORD:
	case CPUS_CREDUI:
		hr = E_NOTIMPL;
		break;

	default:
		hr = E_INVALIDARG;
		break;
	}

	return hr;
}

HANDLE hThread;
bool signatureThreadRunning = true;
char* doAutoLogin;
HANDLE hPipe2;
LPTSTR lpszPipename2 = TEXT("\\\\.\\pipe\\zero");
CSampleProvider* prp;
ICredentialProviderEvents* Pcpe;
UINT_PTR UpAdviseContext;

void getCredentials(char* token)
{
	Log("getCredentials");

	HKEY hKey;
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Juraj Hamornik\\Signature Login\\Token", 0, KEY_ALL_ACCESS, &hKey) != ERROR_SUCCESS)
	{
		Log("Problem with token open");
		delete hKey;
		return; // Token not recognized.
	}

	std::string input(token);
	std::wstring inputw = s2ws(input);

	if (SetVal(hKey, L"Token", inputw) != ERROR_SUCCESS)
	{
		char* const p = reinterpret_cast<char * const>(SetVal(hKey, L"Token", inputw));
		Log("Problem with writing token");
		Log(p);
		delete hKey;
		return; // Token not recognized.
	}

	prp->Pcpe->CredentialsChanged(prp->UpAdviseContext);
}

PWSTR gerPasswd(char* token)
{
	HKEY hKey;
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Juraj Hamornik\\Signature Login\\Keys", 0, KEY_READ, &hKey) != ERROR_SUCCESS)
	{
		delete hKey;
		return (nullptr); // Token not recognized.
	}

	std::wstring keySalt;
	GetStringRegKey(hKey, L"Salt", keySalt, L"bad");
	Log("Salt");
	Log(ws2s(keySalt).c_str());

	CSHA1* sha1 = new CSHA1();
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
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, s2ws(key).c_str(), 0, KEY_READ, &hKey) != ERROR_SUCCESS)
	{
		delete hKey;
		Log("token not recognized second step");
		return (nullptr); // Token not recognized.
	}

	std::wstring salt;

	std::wstring password;

	char* cPassword = s2cs(GetCharRegKey(hKey, L"Password"));

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

	decrypt(cPassword, cDecryptionKey);

	Log("Decripted");
	Log(cPassword);

	password = s2ws(std::string(cPassword));
	wchar_t* wPassword = wcs2cs(password);

	delete cSalt;
	delete cPassword;
	delete cDecryptionKey;

	return wPassword;
}

DWORD WINAPI _NamedPipeReader(LPVOID namedParameter)
{
	BOOL fSuccess;
	char chBuf[100];
	DWORD dwBytesToWrite = (DWORD)strlen(chBuf);
	DWORD cbRead;
	int i;

	char* lastTag = new char[128];
	memset(lastTag, '\0', 128);
	while (lastTag[0] != '\0');
	int tagCount = 0;
	while (signatureThreadRunning)
	{
		Sleep(100);

		fSuccess = ReadFile(hPipe2, chBuf, (DWORD)100, &cbRead, NULL);
		if (fSuccess)
		{
			Log("Success");

			for (i = 0; i < cbRead; i++)
			{
			}

			getCredentials(chBuf);
			break;
		}
	}

	return 0;
}

void InitsignatureReader()
{
	signatureThreadRunning = true;
	hPipe2 = CreateFile(lpszPipename2, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);
	hThread = ::CreateThread(NULL, 0, _NamedPipeReader, NULL, 0, NULL);
}

void StopsignatureReader()
{
	signatureThreadRunning = false;
}

// SetSerialization takes the kind of buffer that you would normally return to LogonUI for
// an authentication attempt.  It's the opposite of ICredentialProviderCredential::GetSerialization.
// GetSerialization is implement by a credential and serializes that credential.  Instead,
// SetSerialization takes the serialization and uses it to create a tile.
//
// SetSerialization is called for two main scenarios.  The first scenario is in the credui case
// where it is prepopulating a tile with credentials that the user chose to store in the OS.
// The second situation is in a remote logon case where the remote client may wish to
// prepopulate a tile with a username, or in some cases, completely populate the tile and
// use it to logon without showing any UI.
//
// If you wish to see an example of SetSerialization, please see either the SampleCredentialProvider
// sample or the SampleCredUICredentialProvider sample.  [The logonUI team says, "The original sample that
// this was built on top of didn't have SetSerialization.  And when we decided SetSerialization was
// important enough to have in the sample, it ended up being a non-trivial amount of work to integrate
// it into the main sample.  We felt it was more important to get these samples out to you quickly than to
// hold them in order to do the work to integrate the SetSerialization changes from SampleCredentialProvider
// into this sample.]
HRESULT CSampleProvider::SetSerialization(
	_In_ CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION const* /*pcpcs*/)
{
	return E_NOTIMPL;
}

// Called by LogonUI to give you a callback.  Providers often use the callback if they
// some event would cause them to need to change the set of tiles that they enumerated.
HRESULT CSampleProvider::Advise(
	_In_ ICredentialProviderEvents* pcpe,
	     _In_ UINT_PTR upAdviseContext
)
{
	InitsignatureReader();
	Pcpe = pcpe;
	Pcpe->AddRef();
	UpAdviseContext = upAdviseContext;
	return E_NOTIMPL;
}

// Called by LogonUI when the ICredentialProviderEvents callback is no longer valid.
HRESULT CSampleProvider::UnAdvise()
{
	StopsignatureReader();

	if (Pcpe)
	{
		Pcpe->Release();
		Pcpe = NULL;
	}
	UpAdviseContext = NULL;

	return S_OK;
}

// Called by LogonUI to determine the number of fields in your tiles.  This
// does mean that all your tiles must have the same number of fields.
// This number must include both visible and invisible fields. If you want a tile
// to have different fields from the other tiles you enumerate for a given usage
// scenario you must include them all in this count and then hide/show them as desired
// using the field descriptors.
HRESULT CSampleProvider::GetFieldDescriptorCount(
	_Out_ DWORD* pdwCount)
{
	*pdwCount = SFI_NUM_FIELDS;
	return S_OK;
}

// Gets the field descriptor for a particular field.
HRESULT CSampleProvider::GetFieldDescriptorAt(
	DWORD dwIndex,
	_Outptr_result_nullonfailure_ CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd)
{
	HRESULT hr;
	*ppcpfd = nullptr;

	// Verify dwIndex is a valid field.
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

// Sets pdwCount to the number of tiles that we wish to show at this time.
// Sets pdwDefault to the index of the tile which should be used as the default.
// The default tile is the tile which will be shown in the zoomed view by default. If
// more than one provider specifies a default the last used cred prov gets to pick
// the default. If *pbAutoLogonWithDefault is TRUE, LogonUI will immediately call
// GetSerialization on the credential you've specified as the default and will submit
// that credential for authentication without showing any further UI.
HRESULT CSampleProvider::GetCredentialCount(
	_Out_ DWORD* pdwCount,
	      _Out_ DWORD* pdwDefault,
	      _Out_ BOOL* pbAutoLogonWithDefault)
{
	*pdwDefault = CREDENTIAL_PROVIDER_NO_DEFAULT;
	*pbAutoLogonWithDefault = TRUE;

	if (_fRecreateEnumeratedCredentials)
	{
		_fRecreateEnumeratedCredentials = false;
		_ReleaseEnumeratedCredentials();
		_CreateEnumeratedCredentials();
	}

	*pdwCount = 1;

	return S_OK;
}

// Returns the credential at the index specified by dwIndex. This function is called by logonUI to enumerate
// the tiles.
HRESULT CSampleProvider::GetCredentialAt(
	DWORD dwIndex,
	_Outptr_result_nullonfailure_ ICredentialProviderCredential** ppcpc)
{
	HRESULT hr = E_INVALIDARG;
	*ppcpc = nullptr;

	if ((dwIndex == 0) && ppcpc)
	{
		hr = _pCredential->QueryInterface(IID_PPV_ARGS(ppcpc));
	}
	return hr;
}

// This function will be called by LogonUI after SetUsageScenario succeeds.
// Sets the User Array with the list of users to be enumerated on the logon screen.
HRESULT CSampleProvider::SetUserArray(_In_ ICredentialProviderUserArray* users)
{
	if (_pCredProviderUserArray)
	{
		_pCredProviderUserArray->Release();
	}
	_pCredProviderUserArray = users;
	_pCredProviderUserArray->AddRef();
	return S_OK;
}

void CSampleProvider::_CreateEnumeratedCredentials()
{
	switch (_cpus)
	{
	case CPUS_LOGON:
	case CPUS_UNLOCK_WORKSTATION:
		{
			_EnumerateCredentials();
			break;
		}
	default:
		break;
	}
}

void CSampleProvider::_ReleaseEnumeratedCredentials()
{
	if (_pCredential != nullptr)
	{
		_pCredential->Release();
		_pCredential = nullptr;
	}
}

HRESULT CSampleProvider::_EnumerateOneCredentials(bool al)
{
	bool _autologin = false;
	PWSTR pwzPassword = NULL;
	Log("EnumerateOneCredentials");

	HKEY hKey;
	if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Juraj Hamornik\\Signature Login\\Token", 0, KEY_ALL_ACCESS, &hKey) != ERROR_SUCCESS)
	{
		delete hKey;
		pwzPassword = NULL;
	}
	else
	{
		std::wstring wToken;
		GetStringRegKey(hKey, L"Token", wToken, L"");
		char* cToken = (char*)s2cs(ws2s(wToken));

		pwzPassword = gerPasswd(cToken);
		RegDeleteValue(hKey, L"Token");
		if (pwzPassword != (nullptr))
		{
			_autologin = true;
		}
	}

	HRESULT hr = E_UNEXPECTED;
	if (_pCredProviderUserArray != nullptr)
	{
		DWORD dwUserCount;
		_pCredProviderUserArray->GetCount(&dwUserCount);
		if (dwUserCount > 0)
		{
			ICredentialProviderUser* pCredUser;
			hr = _pCredProviderUserArray->GetAt(0, &pCredUser);
			if (SUCCEEDED(hr))
			{
				_pCredential = new(std::nothrow) CSampleCredential();
				if (_pCredential != nullptr)
				{
					Log("Initialize _pCredential");
					hr = _pCredential->Initialize(_cpus, s_rgCredProvFieldDescriptors, s_rgFieldStatePairs, pCredUser, _autologin, pwzPassword);

					if (FAILED(hr))
					{
						_pCredential->Release();
						_pCredential = nullptr;
					}
				}
				else
				{
					hr = E_OUTOFMEMORY;
				}

				pCredUser->Release();
			}
		}
	}
	return hr;
}

HRESULT CSampleProvider::_EnumerateCredentials()
{
	HRESULT hr = _EnumerateOneCredentials(false);

	return hr;
}

// Boilerplate code to create our provider.
HRESULT CSample_CreateInstance(_In_ REFIID riid, _Outptr_ void** ppv)
{
	HRESULT hr;
	CSampleProvider* pProvider = new(std::nothrow) CSampleProvider();
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
