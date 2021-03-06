/*
 * Copyright (c) 2008-2015, NVIDIA CORPORATION.  All rights reserved.
 *
 * NVIDIA CORPORATION and its licensors retain all intellectual property
 * and proprietary rights in and to this software, related documentation
 * and any modifications thereto.  Any use, reproduction, disclosure or
 * distribution of this software and related documentation without an express
 * license agreement from NVIDIA CORPORATION is strictly prohibited.
 */

#ifndef LINK_H
#define LINK_H

#include "foundation/PxSimpleTypes.h"
#include "foundation/PxAssert.h"

namespace physx
{
namespace apex
{

class Link
{
public:
	Link()
	{
		adj[1] = adj[0] = this;
	}

	virtual	~Link()
	{
		remove();
	}

	/*
		which = 0:	(-A-...-B-link-)  +  (-this-X-...-Y-)  =  (-A-...-B-link-this-X-...-Y-)
		which = 1:	(-X-...-Y-this-)  +  (-link-A-...-B-)  =  (-X-...-Y-this-link-A-...-B-)
	 */
	void	setAdj(physx::PxU32 which, Link* link)
	{
		physx::PxU32 other = (which &= 1) ^ 1;
		Link* linkAdjOther = link->adj[other];
		adj[which]->adj[other] = linkAdjOther;
		linkAdjOther->adj[which] = adj[which];
		adj[which] = link;
		link->adj[other] = this;
	}

	Link*	getAdj(physx::PxU32 which) const
	{
		return adj[which & 1];
	}

	void	remove()
	{
		adj[1]->adj[0] = adj[0];
		adj[0]->adj[1] = adj[1];
		adj[1] = adj[0] = this;
	}

	bool	isSolitary() const
	{
		PX_ASSERT((adj[0] == this) == (adj[1] == this));
		return adj[0] == this;
	}

protected:
	Link*	adj[2];
};

}
} // end namespace physx::apex


#endif // #ifndef LINK_H
