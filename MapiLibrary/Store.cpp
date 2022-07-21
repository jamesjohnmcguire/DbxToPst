#include "pch.h"

#include "Folder.h"
#include "Store.h"

namespace MapiLibrary
{
	Store::Store(
		LPMAPISESSION mapiSessionIn, ULONG entryIdLengthIn, LPENTRYID entryIdIn)
		:
			mapiSession(mapiSessionIn),
			entryIdLength(entryIdLengthIn),
			entryId(entryIdIn)
	{
	}

	Store::~Store()
	{
	}

	int Store::RemoveDuplicates()
	{
		int duplicatesRemoved = 0;

		HRESULT result = mapiSession->OpenMsgStore(
			0L,
			entryIdLength,
			entryId,
			nullptr,
			MAPI_BEST_ACCESS,
			&mapiDatabase);

		if (result == S_OK)
		{
			unsigned long objectType = 0;

			result = mapiDatabase->OpenEntry(
				0,
				nullptr,
				nullptr,
				MAPI_MODIFY | MAPI_DEFERRED_ERRORS,
				&objectType,
				(LPUNKNOWN*)&rootFolder);

			if (result == S_OK && rootFolder != nullptr)
			{
				std::unique_ptr<Folder> folder =
					std::make_unique<Folder>(rootFolder);

				duplicatesRemoved += folder->RemoveDuplicates();
			}

			if (rootFolder != nullptr)
			{
				rootFolder->Release();
				rootFolder = nullptr;
			}
		}

		return duplicatesRemoved;
	}
}
