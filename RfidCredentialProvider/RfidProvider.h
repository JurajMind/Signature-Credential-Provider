//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
#pragma once

#include <tchar.h>
#include <credentialprovider.h>
#include <windows.h>
#include <strsafe.h>

#include "RfidCredential.h"
#include "helpers.h"

#define MAX_CREDENTIALS 1

RfidCredential *_rgpCredentials[MAX_CREDENTIALS]; // Pointers to the credentials which will be enumerated by 
	                                                           // this Provider.

class RfidProvider : public ICredentialProvider
{
public:
	// IUnknown
	STDMETHOD_(ULONG, AddRef)()
	{
		return _cRef++;
	}

	STDMETHOD_(ULONG, Release)()
	{
		LONG cRef = _cRef--;
		if (!cRef)
		{
			delete this;
		}
		return cRef;
	}

	STDMETHOD (QueryInterface)(REFIID riid, void** ppv)
	{
		HRESULT hr;
		if (IID_IUnknown == riid || 
			IID_ICredentialProvider == riid)
		{
			*ppv = this;
			reinterpret_cast<IUnknown*>(*ppv)->AddRef();
			hr = S_OK;
		}
		else
		{
			*ppv = NULL;
			hr = E_NOINTERFACE;
		}
		return hr;
	}

public:
	IFACEMETHODIMP SetUsageScenario(CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, DWORD dwFlags);
	IFACEMETHODIMP SetSerialization(const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs);

	IFACEMETHODIMP Advise(__in ICredentialProviderEvents* pcpe, UINT_PTR upAdviseContext);
	IFACEMETHODIMP UnAdvise();

	IFACEMETHODIMP GetFieldDescriptorCount(__out DWORD* pdwCount);
	IFACEMETHODIMP GetFieldDescriptorAt(DWORD dwIndex,  __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd);

	IFACEMETHODIMP GetCredentialCount(
		__out DWORD* pdwCount,
		__out DWORD* pdwDefault,
		__out BOOL* pbAutoLogonWithDefault);
	IFACEMETHODIMP GetCredentialAt(
		DWORD dwIndex, 
		__out ICredentialProviderCredential** ppcpc);

	friend HRESULT RfidProvider_CreateInstance(REFIID riid, __deref_out void** ppv);

	// Create/free enumerated credentials.
	HRESULT _EnumerateCredentials();
	void _ReleaseEnumeratedCredentials();
	HRESULT _EnumerateOneCredential(
		__in DWORD dwCredientialIndex,
		__in PWSTR pwzUsername,
		__in PWSTR pwzPassword,
		__in PWSTR pwzDomain
	);
	
	DWORD                                   _dwNumCreds;

	ICredentialProviderEvents* Pcpe;
	UINT_PTR UpAdviseContext;
protected:
	RfidProvider();
	__override ~RfidProvider();

private:


	// ICredentialProviderCredential2
	IFACEMETHODIMP GetUserSid(_Outptr_result_nullonfailure_ PWSTR *ppszSid);

private:

	
	long                                    _cRef;
	CREDENTIAL_PROVIDER_USAGE_SCENARIO      _cpus;                                          // The usage scenario for which we were enumerated.
	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR    _rgCredProvFieldDescriptors[SFI_NUM_FIELDS];    // An array holding the type and name of each field in the tile.
	FIELD_STATE_PAIR                        _rgFieldStatePairs[SFI_NUM_FIELDS];             // An array holding the state of each field in the tile.
	PWSTR                                   _rgFieldStrings[SFI_NUM_FIELDS];                // An array holding the string value of each field. This is different from the name of the field held in _rgCredProvFieldDescriptors.
	PWSTR                                   _pszUserSid;
	PWSTR                                   _pszQualifiedUserName;                          // The user name that's used to pack the authentication buffer
	ICredentialProviderCredentialEvents2*    _pCredProvCredentialEvents;                    // Used to update fields.
	// CredentialEvents2 for Begin and EndFieldUpdates.
	BOOL                                    _fChecked;                                      // Tracks the state of our checkbox.
	DWORD                                   _dwComboIndex;                                  // Tracks the current index of our combobox.
	bool                                    _fShowControls;                                 // Tracks the state of our show/hide controls link.
	bool                                    _fIsLocalUser;                                  // If the cred prov is assosiating with a local user tile
};